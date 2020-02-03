using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions.Fractal
{
    public class FBMFractalFunction : AbstractFractalFunction
    {
        public override float Get(uint? seed, Vector3 coord)
        {
            const float Gain = 0.5f;

            var sum = 0f;
            var amp = 1f;

            var s = seed.HasValue ? seed.Value : Seed;

            for (int i = 0; i < Iterations; ++i)
            {
                var n = BasisFunction.Get(s + (uint)i * 300, coord);
                sum += n * amp;
                amp *= Gain;

                coord *= Lacunarity;
            }

            return sum;
        }
    }
}
