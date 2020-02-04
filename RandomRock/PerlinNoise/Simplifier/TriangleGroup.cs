using PerlinNoise.MeshStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Simplifier
{
    //Store lists of triangles for each vertex group.
    //Also stores lists of triangles that extends to multiple groups.
    internal class TriangleGroup
    {
        private readonly RawMesh _mesh;
        private readonly int[] _vertexGroup;

        private struct Entry
        {
            public int TriangleIndex;
            public int NextEntry;
        }

        private readonly List<Entry> _entries = new List<Entry>();
        private readonly int[] _internalTriangles;
        private readonly int[] _externalTriangles;
        private int _allExternalTriangles = -1;

        public TriangleGroup(RawMesh mesh, int[] vertexGroup, int groupCount)
        {
            _mesh = mesh;
            _vertexGroup = vertexGroup;

            _internalTriangles = Enumerable.Repeat(-1, groupCount).ToArray();
            _externalTriangles = Enumerable.Repeat(-1, groupCount).ToArray();

            for (int i = 0; i < _mesh.Triangles.Length; ++i)
            {
                var tt = _mesh.Triangles[i];
                var ia = vertexGroup[tt.Va];
                var ib = vertexGroup[tt.Vb];
                var ic = vertexGroup[tt.Vc];
                if (ia == ib && ia == ic)
                {
                    Append(i, ref _internalTriangles[ia]);
                }
                else
                {
                    Append(i, ref _externalTriangles[ia]);
                    if (ib != ia)
                    {
                        Append(i, ref _externalTriangles[ib]);
                    }
                    if (ic != ia && ic != ib)
                    {
                        Append(i, ref _externalTriangles[ic]);
                        if (ib != ia)
                        {
                            Append(i, ref _allExternalTriangles);
                        }
                    }
                }
            }
        }

        private void Append(int i, ref int head)
        {
            _entries.Add(new Entry
            {
                TriangleIndex = i,
                NextEntry = head,
            });
            head = _entries.Count - 1;
        }

        public IEnumerable<int> GetInternalTriangles(int group)
        {
            var entry = _internalTriangles[group];
            while (entry != -1)
            {
                yield return _entries[entry].TriangleIndex;
                entry = _entries[entry].NextEntry;
            }
        }

        public IEnumerable<int> GetExternalTriangles(int group)
        {
            var entry = _externalTriangles[group];
            while (entry != -1)
            {
                yield return _entries[entry].TriangleIndex;
                entry = _entries[entry].NextEntry;
            }
        }

        public IEnumerable<int> GetAllExternalTriangles()
        {
            var entry = _allExternalTriangles;
            while (entry != -1)
            {
                yield return _entries[entry].TriangleIndex;
                entry = _entries[entry].NextEntry;
            }
        }
    }
}
