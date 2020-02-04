using PerlinNoise.MeshStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Simplifier
{
    public class MeshSimplifier
    {
        private readonly RawMesh _mesh;
        private readonly float _clusterSize;

        private readonly AbstractVertexGroup _vgroup = new SimpleGridGroup();
        private readonly AbstractGroupMerger _merger = new SimpleAverageGroupMerger();

        public MeshSimplifier(RawMesh mesh, float clusterSize)
        {
            _mesh = mesh;
            _clusterSize = clusterSize;
        }

        public RawMesh Run()
        {
            var groupIndex = new int[_mesh.Vertices.Length];
            _vgroup.MakeGroup(_mesh.Vertices, _clusterSize, groupIndex, out var groupCount);

            var triangleGroup = new TriangleGroup(_mesh, groupIndex, groupCount);
            _merger.SetMeshData(_mesh, groupIndex, groupCount, triangleGroup);

            var newVertices = new Vector3[groupCount];
            for (int i = 0; i < newVertices.Length; ++i)
            {
                newVertices[i] = _merger.GetMergedForGroup(i);
            }

            var newTriangles = new List<RawMesh.Triangle>();
            foreach (var tt in triangleGroup.GetAllExternalTriangles())
            {
                newTriangles.Add(new RawMesh.Triangle
                {
                    Va = groupIndex[_mesh.Triangles[tt].Va],
                    Vb = groupIndex[_mesh.Triangles[tt].Vb],
                    Vc = groupIndex[_mesh.Triangles[tt].Vc],
                });
            }

            //TODO eliminate same triangles (two triangles with same 3 vertices in different order)?

            return new RawMesh
            {
                Vertices = newVertices,
                Triangles = newTriangles.ToArray(),
            };
        }
    }
}
