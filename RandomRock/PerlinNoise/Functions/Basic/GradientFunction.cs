using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions.Basic
{
    public class GradientFunction : IFunction
    {
        public float ReferenceValue;
        public Vector3 ReferencePoint;
        public Vector3 Increase;

        public float Get(uint? seed, Vector3 coord)
        {
            return ReferenceValue + Vector3.Dot(coord - ReferencePoint, Increase);
        }
    }
}
