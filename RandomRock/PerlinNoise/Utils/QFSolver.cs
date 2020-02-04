using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Utils
{
    internal class QFSolver
    {
        public static Vector3 Solve(QFCoefficient coefficient)
        {
            //https://www.value-at-risk.net/minimizing-a-quadratic-polynomial/
            //Minimize x' c x + b x + a
            //c:
            //x^2 xy  xz
            //    y^2 yz
            //        z^2
            var vv = coefficient.Values;

            Matrix4x4 c = new Matrix4x4(
                vv[0], vv[3] / 2, vv[5] / 2, 0,
                vv[3] / 2, vv[1], vv[4] / 2, 0,
                vv[5] / 2, vv[4] / 2, vv[2], 0,
                0, 0, 0, 1);
            if (!Matrix4x4.Invert(c, out var ic))
            {
                throw new Exception("Internal error");
            }
            var b = new Vector4(vv[6], vv[7], vv[8], 0);

            var ret = Vector4.Transform(b, Matrix4x4.Transpose(ic)) * -0.5f;
            return new Vector3(ret.X, ret.Y, ret.Z);
        }
    }
}
