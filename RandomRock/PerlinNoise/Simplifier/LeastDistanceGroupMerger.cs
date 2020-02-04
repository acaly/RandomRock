using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using PerlinNoise.MeshStorage;
using PerlinNoise.Utils;

namespace PerlinNoise.Simplifier
{
    class LeastDistanceGroupMerger : AbstractGroupMerger
    {
        private Vector3[] _results;

        public override Vector3 GetMergedForGroup(int group)
        {
            return _results[group];
        }

        public override void SetMeshData(RawMesh mesh, int[] vertexGroup, int groupCount, TriangleGroup triangleGroup)
        {
            //Calculate average for bias.
            var sum = new Vector3[groupCount];
            var count = new int[groupCount];

            for (int i = 0; i < mesh.Vertices.Length; ++i)
            {
                var g = vertexGroup[i];
                sum[g] += mesh.Vertices[i];
                count[g] += 1;
            }

            //Run qf solver
            _results = new Vector3[groupCount];
            var c = new QFCoefficient();
            
            for (int i = 0; i < groupCount; ++i)
            {
                c.Reset();
                var nFaces = 0;
                var vertices = Enumerable.Range(0, vertexGroup.Length).Where(ii => vertexGroup[ii] == i).ToArray();
                foreach (var tt in triangleGroup.GetExternalTriangles(i))
                {
                    var v1 = mesh.Vertices[mesh.Triangles[tt].Va];
                    var v2 = mesh.Vertices[mesh.Triangles[tt].Vb];
                    var v3 = mesh.Vertices[mesh.Triangles[tt].Vc];
                    var n = Vector3.Normalize(Vector3.Cross(v2 - v1, v3 - v1));
                    c.AddPlane(v1, n);
                    nFaces += 1;
                }
                c.AddBias(sum[i] / count[i], nFaces * 0.01f + 0.01f);
                _results[i] = QFSolver.Solve(c);
            }
        }
    }
}
