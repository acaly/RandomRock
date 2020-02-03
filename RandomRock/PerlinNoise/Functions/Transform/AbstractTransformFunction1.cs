using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions.Transform
{
    public abstract class AbstractTransformFunction1 : IFunction
    {
        public IFunction Input { get; set; }

        public float Get(uint? seed, Vector3 coord)
        {
            return Tranform(Input.Get(null, coord));
        }

        protected abstract float Tranform(float input);
    }
}
