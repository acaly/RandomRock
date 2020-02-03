using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Models
{
    public class NormalModel
    {
        public struct Vertex
        {
            public int Position;
            public Vector3 Normal;
        }

        public struct Triangle
        {
            public int Va, Vb, Vc;
        }

        public Vector3[] Positions;
        public Vertex[] Vertices;
        public Triangle[] Triangles;

        public static NormalModel MakeNormal(RawModel r)
        {
            var ret = new NormalModel
            {
                Positions = r.Vertices.ToArray(),
                Triangles = new Triangle[r.Triangles.Length],
            };
            var vlist = new List<Vertex>();
            for (var i = 0; i < r.Triangles.Length; ++i)
            {
                var v1 = r.Triangles[i].Va;
                var v2 = r.Triangles[i].Vb;
                var v3 = r.Triangles[i].Vc;
                var nn = Vector3.Normalize(Vector3.Cross(r.Vertices[v2] - r.Vertices[v1], r.Vertices[v3] - r.Vertices[v1]));
                ret.Triangles[i] = new Triangle
                {
                    Va = vlist.Count,
                    Vb = vlist.Count + 1,
                    Vc = vlist.Count + 2,
                };
                vlist.Add(new Vertex { Position = v1, Normal = nn });
                vlist.Add(new Vertex { Position = v2, Normal = nn });
                vlist.Add(new Vertex { Position = v3, Normal = nn });
            }
            ret.Vertices = vlist.ToArray();
            return ret;
        }
    }
}
