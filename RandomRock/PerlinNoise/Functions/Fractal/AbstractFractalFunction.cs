using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions.Fractal
{
    public abstract class AbstractFractalFunction : IFunction
    {
        public IFunction BasisFunction { get; set; }
        public uint Seed { get; set; }
        public int Iterations { get; set; } = 7;
        public float Lacunarity { get; set; } = 2f;

        public abstract float Get(uint? seed, Vector3 coord);
    }
}
