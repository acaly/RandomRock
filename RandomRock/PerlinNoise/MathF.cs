using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise
{
    internal class MathF
    {
        public static int FloorI(float v)
        {
            var iv = (int)v;
            return v >= 0 ? iv : iv - 1;
        }
    }
}
