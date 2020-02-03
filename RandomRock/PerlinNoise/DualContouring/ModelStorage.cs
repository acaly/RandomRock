using PerlinNoise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.DualContouring
{
    internal class ModelStorage
    {
        private readonly int _size;

        //key: low direction grid coordinate. For example, (0.5, 0.5, 0.5) is stored with key (0, 0, 0)
        private readonly Dictionary<long, int> _gridVertex = new Dictionary<long, int>();

        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<int> _indices = new List<int>();

        public ModelStorage(int size)
        {
            _size = size;
        }

        public void AddVertex(Vector3 v)
        {
            var grid = GridCoordinate.GetGridForPoint(v);
            
            var key = grid.GetGridLong(_size);
            if (_gridVertex.ContainsKey(key)) return;

            _gridVertex.Add(key, _vertices.Count);
            _vertices.Add(v);
        }

        private int GetVertex(GridCoordinate grid)
        {
            return _gridVertex[grid.GetGridLong(_size)];
        }

        //same as DCSolver._edgeCoordToAdjacentGrid
        private static readonly GridCoordinate[] _axisGridOffsetList = new []
        {
            new GridCoordinate(0, -1, -1), new GridCoordinate(0, 0, -1), new GridCoordinate(0, -1, 0),
            new GridCoordinate(-1, 0, -1), new GridCoordinate(-1, 0, 0), new GridCoordinate(0, 0, -1),
            new GridCoordinate(-1, -1, 0), new GridCoordinate(0, -1, 0), new GridCoordinate(-1, 0, 0),
        };

        public void AddFace(GridEdge edge, bool reversedNormal)
        {
            var grid = edge.Coord;
            var axis = edge.Axis;

            int p0 = GetVertex(grid + _axisGridOffsetList[axis * 3 + 0]);
            int p1 = GetVertex(grid + _axisGridOffsetList[axis * 3 + 1]);
            int p2 = GetVertex(grid + _axisGridOffsetList[axis * 3 + 2]);
            int p3 = GetVertex(grid);
            if (!reversedNormal)
            {
                _indices.Add(p0);
                _indices.Add(p1);
                _indices.Add(p3);
                _indices.Add(p0);
                _indices.Add(p3);
                _indices.Add(p2);
            }
            else
            {
                _indices.Add(p0);
                _indices.Add(p3);
                _indices.Add(p1);
                _indices.Add(p0);
                _indices.Add(p2);
                _indices.Add(p3);
            }
        }

        public RawModel Generate()
        {
            var ret = new RawModel
            {
                Vertices = _vertices.ToArray(),
                Triangles = new RawModel.Triangle[_indices.Count / 3],
            };
            for (var i = 0; i < ret.Triangles.Length; ++i)
            {
                ret.Triangles[i] = new RawModel.Triangle
                {
                    Va = _indices[i * 3 + 0],
                    Vb = _indices[i * 3 + 1],
                    Vc = _indices[i * 3 + 2],
                };
            }
            return ret;
        }
    }
}
