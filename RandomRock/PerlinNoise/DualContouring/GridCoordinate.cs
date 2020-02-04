using PerlinNoise.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.DualContouring
{
    internal struct GridCoordinate
    {
        public int X, Y, Z;

        public static readonly GridCoordinate Zero = new GridCoordinate();

        public GridCoordinate(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static GridCoordinate GetGridForPoint(Vector3 p)
        {
            return new GridCoordinate(MathF.FloorI(p.X), MathF.FloorI(p.Y), MathF.FloorI(p.Z));
        }

        public long GetGridLong(int size)
        {
            long ls = size;
            return X + ls * (Y + ls * Z);
        }

        public Vector3 ToVector()
        {
            return new Vector3(X, Y, Z);
        }

        public static GridCoordinate operator +(GridCoordinate a, GridCoordinate b)
        {
            return new GridCoordinate(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static GridCoordinate operator -(GridCoordinate a, GridCoordinate b)
        {
            return new GridCoordinate(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
    }
}
