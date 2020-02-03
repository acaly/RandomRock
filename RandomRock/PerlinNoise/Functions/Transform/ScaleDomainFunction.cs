using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions.Transform
{
    public class ScaleDomainFunction : IFunction
    {
        public IFunction Input { get; set; }
        public float Ratio { get; set; }

        public float Get(uint? seed, Vector3 coord)
        {
            return Input.Get(null, coord * Ratio);
        }
    }
}
