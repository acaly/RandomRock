using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions.Basic
{
    public class DistanceFunction : IFunction
    {
        public Vector3 Reference { get; set; }

        public float Get(uint? seed, Vector3 coord)
        {
            return Vector3.Distance(coord, Reference);
        }
    }
}
