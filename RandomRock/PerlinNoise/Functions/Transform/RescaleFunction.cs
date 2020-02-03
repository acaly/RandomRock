using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions.Transform
{
    public class RescaleFunction : AbstractTransformFunction1
    {
        public float K, B;

        protected override float Tranform(float input)
        {
            return input * K + B;
        }
    }
}
