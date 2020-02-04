using PerlinNoise.MeshStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Simplifier
{
    internal abstract class AbstractGroupMerger
    {
        public abstract void SetMeshData(RawMesh mesh, int[] vertexGroup, int groupCount, TriangleGroup triangleGroup);
        public abstract Vector3 GetMergedForGroup(int group);
    }
}
