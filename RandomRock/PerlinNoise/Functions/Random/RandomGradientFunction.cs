using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions.Random
{
    public class RandomGradientFunction : AbstractRandomFunction
    {
        public HashedRandomNormal Normal = HashedRandomNormal.Straight;

        protected override float NoiseFunction(Vector3 coord, int ix, int iy, int iz, uint seed)
        {
            var hash = Hashing.XorHash(ix, iy, iz, (int)seed);
            var normal = Normal.Values[hash];
            var icoord = new Vector3(ix, iy, iz);
            return Vector3.Dot(normal, coord - icoord);
        }
    }
}
