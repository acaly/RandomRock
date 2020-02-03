using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions.Random
{
    public sealed class RandomValueFunction : AbstractRandomFunction
    {
        protected override float NoiseFunction(Vector3 coord, int ix, int iy, int iz, uint seed)
        {
            var hash = Hashing.XorHash(ix, iy, iz, (int)seed);
            return hash / (255f / 2) - 1;
        }
    }
}
