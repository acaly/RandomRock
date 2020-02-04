using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using PerlinNoise.MeshStorage;

namespace PerlinNoise.Simplifier
{
    internal class SimpleAverageGroupMerger : AbstractGroupMerger
    {
        private Vector3[] _sum;
        private int[] _count;

        public override Vector3 GetMergedForGroup(int group)
        {
            //_count cannot be zero (we eliminated empty groups).
            return _sum[group] / _count[group];
        }

        public override void SetMeshData(RawMesh mesh, int[] vertexGroup, int groupCount, TriangleGroup triangleGroup)
        {
            _sum = new Vector3[groupCount];
            _count = new int[groupCount];

            for (int i = 0; i < mesh.Vertices.Length; ++i)
            {
                var g = vertexGroup[i];
                _sum[g] += mesh.Vertices[i];
                _count[g] += 1;
            }
        }
    }
}
