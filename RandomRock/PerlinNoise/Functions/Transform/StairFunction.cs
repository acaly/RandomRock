using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions.Transform
{
    public class StairFunction : AbstractTransformFunction1
    {
        public float Threshold { get; set; }
        public float High { get; set; }
        public float Low { get; set; }

        protected override float Tranform(float input)
        {
            return input >= Threshold ? High : Low;
        }
    }
}
