using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Simplifier
{
    internal abstract class AbstractVertexGroup
    {
        public abstract void MakeGroup(Vector3[] vertices, float groupSize, int[] groupIndex, out int groupCount);
    }
}
