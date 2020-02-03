using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions.Random
{
    public abstract class AbstractRandomFunction : IFunction
    {
        public IInterpolation Interpolation { get; set; } = new NearestInterpolation();

        public uint Seed { get; set; }

        public float Get(uint? seed, Vector3 coord)
        {
            var ix = MathF.FloorI(coord.X);
            var iy = MathF.FloorI(coord.Y);
            var iz = MathF.FloorI(coord.Z);
            var dx = Interpolation.Interpolate(coord.X - ix);
            var dy = Interpolation.Interpolate(coord.Y - iy);
            var dz = Interpolation.Interpolate(coord.Z - iz);

            var s = seed.HasValue ? seed.Value : Seed;
            var v000 = NoiseFunction(coord, ix + 0, iy + 0, iz + 0, s);
            var v100 = NoiseFunction(coord, ix + 1, iy + 0, iz + 0, s);
            var v010 = NoiseFunction(coord, ix + 0, iy + 1, iz + 0, s);
            var v110 = NoiseFunction(coord, ix + 1, iy + 1, iz + 0, s);
            var v001 = NoiseFunction(coord, ix + 0, iy + 0, iz + 1, s);
            var v101 = NoiseFunction(coord, ix + 1, iy + 0, iz + 1, s);
            var v011 = NoiseFunction(coord, ix + 0, iy + 1, iz + 1, s);
            var v111 = NoiseFunction(coord, ix + 1, iy + 1, iz + 1, s);

            var vi00 = Lerp(v000, v100, dx);
            var vi10 = Lerp(v010, v110, dx);
            var vi01 = Lerp(v001, v101, dx);
            var vi11 = Lerp(v011, v111, dx);

            var vii0 = Lerp(vi00, vi10, dy);
            var vii1 = Lerp(vi01, vi11, dy);

            return Lerp(vii0, vii1, dz);
        }

        private static float Lerp(float v0, float v1, float x)
        {
            return (1 - x) * v0 + x * v1;
        }

        protected abstract float NoiseFunction(Vector3 coord, int ix, int iy, int iz, uint seed);
    }
}
