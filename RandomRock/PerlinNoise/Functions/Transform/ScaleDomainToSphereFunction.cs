using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions.Transform
{
    public class ScaleDomainToSphereFunction : IFunction
    {
        public IFunction Input { get; set; }
        public float Radius { get; set; }
        public float Significance { get; set; } = 0.3f;

        public float Get(uint? seed, Vector3 coord)
        {
            var len2 = coord.LengthSquared();
            var lentr = (float)Math.Pow(len2, -0.5 + Significance * 0.5);
            return Input.Get(null, coord * lentr);
        }
    }
}
