using PerlinNoise.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.DualContouring
{
    //TODO Currently we have no cache
    internal class GridCache
    {
        private readonly IFunction _function;
        private readonly int _size;

        public GridCache(IFunction function, int maxSize)
        {
            _function = function;
            _size = maxSize;
        }

        public float EvaluateAtGrid(GridCoordinate coord)
        {
            return _function.Get(null, coord.ToVector());
        }

        private float EvaluateDifference(Vector3 pos, Vector3 diff)
        {
            return _function.Get(null, pos + diff) - _function.Get(null, pos - diff);
        }

        public Vector3 EvaluateGradient(Vector3 pos)
        {
            const float distance = 0.01f;
            var x = EvaluateDifference(pos, new Vector3(distance, 0, 0));
            var y = EvaluateDifference(pos, new Vector3(0, distance, 0));
            var z = EvaluateDifference(pos, new Vector3(0, 0, distance));
            return new Vector3(x, y, z);
        }
    }
}
