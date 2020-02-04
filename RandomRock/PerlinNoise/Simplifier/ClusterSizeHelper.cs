using PerlinNoise.MeshStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Simplifier
{
    public class ClusterSizeHelper
    {
        private static float MinDistance(RawMesh mesh, int t)
        {
            var va = mesh.Vertices[mesh.Triangles[t].Va];
            var vb = mesh.Vertices[mesh.Triangles[t].Vb];
            var vc = mesh.Vertices[mesh.Triangles[t].Vc];
            var da = (vb - vc).LengthSquared();
            var db = (vc - va).LengthSquared();
            var dc = (va - vb).LengthSquared();
            return (float)Math.Sqrt(Math.Min(da, Math.Min(db, dc)));
        }

        private static Random _rand = new Random();

        //Warning: not thread safe!
        public static float GetSampleAverage(RawMesh mesh, float ratio = 2f, int sample = 3)
        {
            var total = 0f;
            for (int s = 0; s < sample; ++s)
            {
                var tt = _rand.Next(mesh.Triangles.Length);
                total += MinDistance(mesh, tt);
            }
            return total / sample * ratio;
        }
    }
}
