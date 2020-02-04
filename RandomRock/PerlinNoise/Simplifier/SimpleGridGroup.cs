using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Simplifier
{
    internal class SimpleGridGroup : AbstractVertexGroup
    {
        private List<long> _longGroupList = new List<long>();
        private HashSet<long> _usedLongGrupId = new HashSet<long>();
        private Dictionary<long, int> _longGoupAssignment = new Dictionary<long, int>();

        public override void MakeGroup(Vector3[] vertices, float groupSize, int[] groupIndex, out int groupCount)
        {
            //Scan range
            var min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

            foreach (var vv in vertices)
            {
                if (vv.X < min.X) min.X = vv.X;
                if (vv.Y < min.Y) min.Y = vv.Y;
                if (vv.Z < min.Z) min.Z = vv.Z;
                if (vv.X > max.X) max.X = vv.X;
                if (vv.Y > max.Y) max.Y = vv.Y;
                if (vv.Z > max.Z) max.Z = vv.Z;
            }

            var invGroupSize = 1 / groupSize;

            var nx = (int)Math.Ceiling((max.X - min.X) * invGroupSize);
            var ny = (int)Math.Ceiling((max.Y - min.Y) * invGroupSize);

            //Calculate long group for each vertex
            _longGroupList.Clear();
            _usedLongGrupId.Clear();

            foreach (var vv in vertices)
            {
                var ix = (long)Math.Floor((vv.X - min.X) * invGroupSize);
                var iy = (long)Math.Floor((vv.Y - min.Y) * invGroupSize);
                var iz = (long)Math.Floor((vv.Z - min.Z) * invGroupSize);

                var g = ix + nx * (iy + ny * iz);
                _longGroupList.Add(g);
                _usedLongGrupId.Add(g);
            }
            groupCount = _usedLongGrupId.Count;

            //Assign group id
            foreach (var gg in _usedLongGrupId)
            {
                _longGoupAssignment.Add(gg, _longGoupAssignment.Count);
            }

            //Update result array
            for (int i = 0; i < vertices.Length; ++i)
            {
                groupIndex[i] = _longGoupAssignment[_longGroupList[i]];
            }
        }
    }
}
