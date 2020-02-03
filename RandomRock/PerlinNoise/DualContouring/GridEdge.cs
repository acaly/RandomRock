using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.DualContouring
{
    internal struct GridEdge
    {
        public GridCoordinate Coord;
        public int Axis;

        public GridEdge(GridCoordinate coord, int axis)
        {
            Coord = coord;
            Axis = axis;
        }

        public long GetEdgeLong(int size)
        {
            return Coord.GetGridLong(size) * 3 + Axis;
        }

        private static readonly GridCoordinate[] _offset = new[]
        {
            new GridCoordinate(1, 0, 0), new GridCoordinate(0, 1, 0), new GridCoordinate(0, 0, 1),
        };

        public GridCoordinate Low => Coord;
        public GridCoordinate High => Coord + _offset[Axis];
    }
}
