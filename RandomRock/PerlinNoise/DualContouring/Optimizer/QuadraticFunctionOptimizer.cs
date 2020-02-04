using PerlinNoise.Utils;
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
        private QFCoefficient _qfCoefficient;

        private static Vector3 SolveQF(QFCoefficient coefficient)
        {
            const float constraint = 0.7f;
            const float constraintMax = 1 + constraint, constraintMin = 1 - constraint;

            var p = QFSolver.Solve(coefficient);
            var dx = Math.Abs(p.X - 1);
            var dy = Math.Abs(p.Y - 1);
            var dz = Math.Abs(p.Z - 1);

            var maxd = Math.Max(dx, Math.Max(dy, dz));
            if (maxd <= constraint) return p;

            Vector3 ret = new Vector3();
            float val = float.PositiveInfinity;

            if (dx > constraint)
            {
                var nco = coefficient.Clone();
                nco.EliminateDimension(0, p.X > 1 ? constraintMax : constraintMin);
                var np = SolveQF(nco);
                np.X = p.X > 1 ? constraintMax : constraintMin;
                var nval = nco.Evaluate(np);
                if (val > nval)
                {
                    ret = np;
                    val = nval;
                }
            }
            if (dy > constraint)
            {
                var nco = coefficient.Clone();
                nco.EliminateDimension(1, p.Y > 1 ? constraintMax : constraintMin);
                var np = SolveQF(nco);
                np.Y = p.Y > 1 ? constraintMax : constraintMin;
                var nval = nco.Evaluate(np);
                if (val > nval)
                {
                    ret = np;
                    val = nval;
                }
            }
            if (dz > constraint)
            {
                var nco = coefficient.Clone();
                nco.EliminateDimension(2, p.Z > 1 ? constraintMax : constraintMin);
                var np = SolveQF(nco);
                np.Z = p.Z > 1 ? constraintMax : constraintMin;
                var nval = nco.Evaluate(np);
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
            _qfCoefficient.Reset(new Vector3(1, 1, 1), 0.1f);
        }

        public override void AddCrossEdge(Vector3 position, Vector3 gradient)
        {
            _qfCoefficient.AddPlane(position, gradient);
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
