using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Utils
{
    internal class QFCoefficient
    {
        public float[] Values = new float[9];

        //For x: xx xy xz x y z
        private static readonly int[][] _coefficientAxisMap = new[]
        {
            new [] { 0, 3, 5, 6, 7, 8 },
            new [] { 1, 4, 3, 7, 8, 6 },
            new [] { 2, 5, 4, 8, 6, 7 },
        };

        public float Evaluate(Vector3 p)
        {
            return Values[0] * p.X * p.X +
                Values[1] * p.Y * p.Y +
                Values[2] * p.Z * p.Z +
                Values[3] * p.X * p.Y +
                Values[4] * p.Y * p.Z +
                Values[5] * p.Z * p.X +
                Values[6] * p.X +
                Values[7] * p.Y +
                Values[8] * p.Z;
        }

        public void Reset(Vector3 bias, float biasMagnitude)
        {
            Array.Clear(Values, 0, Values.Length);

            if (biasMagnitude > 0)
            {
                SetAxisConst(0, bias.X, biasMagnitude);
                SetAxisConst(1, bias.Y, biasMagnitude);
                SetAxisConst(2, bias.Z, biasMagnitude);
            }
        }

        private void SetAxisConst(int axis, float val, float mag)
        {
            var index = _coefficientAxisMap[axis];
            Values[index[0]] = mag;
            Values[index[3]] = -2 * mag * val;
        }

        public QFCoefficient Clone()
        {
            var c = new QFCoefficient();
            var ret = c.Values;
            Array.Copy(Values, 0, ret, 0, Values.Length);
            return c;
        }

        public void EliminateDimension(int axis, float val)
        {
            var index = _coefficientAxisMap[axis];

            //xy xz -> y z
            Values[index[4]] += Values[index[1]] * val;
            Values[index[5]] += Values[index[2]] * val;

            //clear xy xz
            Values[index[1]] = 0;
            Values[index[2]] = 0;

            //add x^2-2x (let x=1)
            Values[index[0]] = 1;
            Values[index[3]] = -2;
        }

        public void AddPlane(Vector3 r, Vector3 g)
        {
            //dot(p - r, g) ^ 2
            //[ (x - rx) * gx + (y - ry) * gy + (z - rz) * gz ] ^ 2
            //[ gx * x + gy * y + gz * z - rx * gx - ry * gy - rz * gz ] ^2
            //[ A x + B y + C z + D] ^ 2
            var A = g.X;
            var B = g.Y;
            var C = g.Z;
            var D = -Vector3.Dot(g, r);
            Values[0] += A * A; //x^2
            Values[1] += B * B; //y*2
            Values[2] += C * C; //z*2
            Values[3] += 2 * A * B; //xy
            Values[4] += 2 * B * C; //yz
            Values[5] += 2 * C * A; //zx
            Values[6] += 2 * A * D; //x
            Values[7] += 2 * B * D; //y
            Values[8] += 2 * C * D; //z
        }
    }
}
