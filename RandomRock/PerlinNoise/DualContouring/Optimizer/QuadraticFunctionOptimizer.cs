using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.DualContouring.Optimizer
{
    class QuadraticFunctionOptimizer : AbstractOptimizer
    {
        private float[] _qfCoefficient = new float[10];

        private const float _gridVertexCenterBias = .1f;
        private static readonly float[] _qfCoefficient0 = new float[]
        {
            _gridVertexCenterBias, _gridVertexCenterBias, _gridVertexCenterBias,
            0, 0, 0,
            -2 * _gridVertexCenterBias, -2 * _gridVertexCenterBias, -2 * _gridVertexCenterBias,
            0,
        };

        private static void AddCoefficient(float[] coefficient, Vector3 r, Vector3 g)
        {
            //dot(p - r, g) ^ 2
            //[ (x - rx) * gx + (y - ry) * gy + (z - rz) * gz ] ^ 2
            //[ gx * x + gy * y + gz * z - rx * gx - ry * gy - rz * gz ] ^2
            //[ A x + B y + C z + D] ^ 2
            var A = g.X;
            var B = g.Y;
            var C = g.Z;
            var D = -Vector3.Dot(g, r);
            coefficient[0] += A * A; //x^2
            coefficient[1] += B * B; //y*2
            coefficient[2] += C * C; //z*2
            coefficient[3] += 2 * A * B; //xy
            coefficient[4] += 2 * B * C; //yz
            coefficient[5] += 2 * C * A; //zx
            coefficient[6] += 2 * A * D; //x
            coefficient[7] += 2 * B * D; //y
            coefficient[8] += 2 * C * D; //z
            coefficient[9] += D * D; //1
        }

        private static Vector3 SolveQFSingle(float[] coefficient)
        {
            //https://www.value-at-risk.net/minimizing-a-quadratic-polynomial/
            //Minimize x' c x + b x + a
            //c:
            //x^2 xy  xz
            //    y^2 yz
            //        z^2
            Matrix4x4 c = new Matrix4x4(
                coefficient[0], coefficient[3], coefficient[5], 0,
                0, coefficient[1], coefficient[4], 0,
                0, 0, coefficient[2], 0,
                0, 0, 0, 1);
            if (!Matrix4x4.Invert(c, out var ic))
            {
                throw new Exception("Internal error");
            }
            var b = new Vector4(coefficient[6], coefficient[7], coefficient[8], 0);

            var ret = Vector4.Transform(b, ic) * -0.5f;
            return new Vector3(ret.X, ret.Y, ret.Z);
        }

        //For x, xx xy xz x y z
        private static readonly int[][] _coefficientAxisMap = new[]
        {
            new [] { 0, 3, 5, 6, 7, 8 },
            new [] { 1, 4, 3, 7, 8, 6 },
            new [] { 2, 5, 4, 8, 6, 7 },
        };

        private static float[] ApplyPosition(float[] coefficient, float val, int axis)
        {
            var index = _coefficientAxisMap[axis];

            var ret = new float[coefficient.Length];
            Array.Copy(coefficient, 0, ret, 0, ret.Length);

            //xy xz -> y z
            ret[index[4]] += coefficient[index[1]] * val;
            ret[index[5]] += coefficient[index[2]] * val;

            //clear xy xz
            ret[index[1]] = 0;
            ret[index[2]] = 0;

            //add x^2-2x (let x=1)
            ret[index[0]] = 1;
            ret[index[3]] = -2;

            return ret;
        }

        private static float EvaluateQF(float[] coefficient, Vector3 p)
        {
            return coefficient[0] * p.X * p.X +
                coefficient[1] * p.Y * p.Y +
                coefficient[2] * p.Z * p.Z +
                coefficient[3] * p.X * p.Y +
                coefficient[4] * p.Y * p.Z +
                coefficient[5] * p.Z * p.X +
                coefficient[6] * p.X +
                coefficient[7] * p.Y +
                coefficient[8] * p.Z;
        }

        private static Vector3 SolveQF(float[] coefficient)
        {
            const float constraint = 0.7f;
            const float constraintMax = 1 + constraint, constraintMin = 1 - constraint;

            var p = SolveQFSingle(coefficient);
            var dx = Math.Abs(p.X - 1);
            var dy = Math.Abs(p.Y - 1);
            var dz = Math.Abs(p.Z - 1);

            var maxd = Math.Max(dx, Math.Max(dy, dz));
            if (maxd <= constraint) return p;

            Vector3 ret = new Vector3();
            float val = float.PositiveInfinity;

            if (dx > constraint)
            {
                var nco = ApplyPosition(coefficient, p.X > 1 ? constraintMax : constraintMin, 0);
                var np = SolveQF(nco);
                np.X = p.X > 1 ? constraintMax : constraintMin;
                var nval = EvaluateQF(coefficient, np);
                if (val > nval)
                {
                    ret = np;
                    val = nval;
                }
            }
            if (dy > constraint)
            {
                var nco = ApplyPosition(coefficient, p.Y > 1 ? constraintMax : constraintMin, 1);
                var np = SolveQF(nco);
                np.Y = p.Y > 1 ? constraintMax : constraintMin;
                var nval = EvaluateQF(coefficient, np);
                if (val > nval)
                {
                    ret = np;
                    val = nval;
                }
            }
            if (dz > constraint)
            {
                var nco = ApplyPosition(coefficient, p.Z > 1 ? constraintMax : constraintMin, 2);
                var np = SolveQF(nco);
                np.Z = p.Z > 1 ? constraintMax : constraintMin;
                var nval = EvaluateQF(coefficient, np);
                if (val > nval)
                {
                    ret = np;
                    val = nval;
                }
            }
            return ret;
        }

        public override void Reset()
        {
            Array.Copy(_qfCoefficient0, 0, _qfCoefficient, 0, _qfCoefficient.Length);
        }

        public override void AddCrossEdge(Vector3 position, Vector3 gradient)
        {
            AddCoefficient(_qfCoefficient, position, gradient);
        }

        public override Vector3 Optimize()
        {
            var xx = SolveQF(_qfCoefficient);

            ////If outside range, choose the nearest point.
            //var x = Math.Max(0.1f, Math.Min(0.9f, xx.X));
            //var y = Math.Max(0.1f, Math.Min(0.9f, xx.Y));
            //var z = Math.Max(0.1f, Math.Min(0.9f, xx.Z));
            var x = Math.Abs(xx.X - 0.5f) < 0.5f ? xx.X : 0.5f;
            var y = Math.Abs(xx.Y - 0.5f) < 0.5f ? xx.Y : 0.5f;
            var z = Math.Abs(xx.Z - 0.5f) < 0.5f ? xx.Z : 0.5f;

            return new Vector3(x, y, z);
        }
    }
}
