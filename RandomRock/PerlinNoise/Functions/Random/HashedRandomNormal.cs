using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions.Random
{
    public class HashedRandomNormal
    {
        internal Vector3[] Values;

        public static readonly HashedRandomNormal Straight, Arbitrary;

        static HashedRandomNormal()
        {
            Straight = new HashedRandomNormal
            {
                Values = new Vector3[256 + 6],
            };
            for (int i = 0; i < 256; i += 6)
            {
                Straight.Values[i + 0] = new Vector3(-1, 0, 0);
                Straight.Values[i + 1] = new Vector3(1, 0, 0);
                Straight.Values[i + 2] = new Vector3(0, -1, 0);
                Straight.Values[i + 3] = new Vector3(0, 1, 0);
                Straight.Values[i + 4] = new Vector3(0, 0, -1);
                Straight.Values[i + 5] = new Vector3(0, 0, 1);
            }

            Arbitrary = new HashedRandomNormal
            {
                Values = new Vector3[256],
            };
            for (int i = 0; i < 16; ++i)
            {
                for (int j = 0; j < 16; ++j)
                {
                    var fi = (i + 0.5f) / 16;
                    var fj = (j + 0.5f) / 16;
                    var z = fi * 2 - 1;
                    var xy = Math.Sqrt(1 - z * z);
                    var x = xy * Math.Sin(fj * 2 * Math.PI);
                    var y = xy * Math.Cos(fj * 2 * Math.PI);
                    Arbitrary.Values[i * 16 + j] = new Vector3((float)x, (float)y, z);
                }
            }
        }
    }
}
