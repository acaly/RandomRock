using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Models
{
    public class RawModel
    {
        public struct Triangle
        {
            public int Va, Vb, Vc;
        }

        public Vector3[] Vertices;
        public Triangle[] Triangles;
    }
}
