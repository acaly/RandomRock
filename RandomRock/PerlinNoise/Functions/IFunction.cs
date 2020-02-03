using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions
{
    public interface IFunction
    {
        float Get(uint? seed, Vector3 coord);
    }
}
