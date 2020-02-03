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
        private readonly int _cacheCount;

        private struct GridInfo { public float Val; public long Time; }
        private readonly SortedDictionary<long, long> _timeGrid = new SortedDictionary<long, long>();
        private readonly Dictionary<long, GridInfo> _gridValue = new Dictionary<long, GridInfo>();
        private long _nextTime = 0;

        private void AddToCache(long grid, float val)
        {
            if (_timeGrid.Count >= _cacheCount)
            {
                var lru = _timeGrid.First();
                _timeGrid.Remove(lru.Key);
                _gridValue.Remove(lru.Value);
            }
            var t = _nextTime++;
            _timeGrid[t] = grid;
            _gridValue[grid] = new GridInfo { Val = val, Time = t };
        }

        private float? ReadCache(long grid)
        {
            if (!_gridValue.TryGetValue(grid, out var val))
            {
                return null;
            }
            var t = _nextTime++;
            val.Time = t;
            _gridValue[grid] = val;
            return val.Val;
        }

        public GridCache(IFunction function, int maxSize)
        {
            _function = function;
            _size = maxSize;
            _cacheCount = maxSize * 20 + 20;
        }

        public float EvaluateAtGrid(GridCoordinate coord)
        {
            var g = coord.GetGridLong(_size);
            var tryRead = ReadCache(g);
            if (tryRead.HasValue)
            {
                return tryRead.Value;
            }
            else
            {
                var val = _function.Get(null, coord.ToVector());
                AddToCache(g, val);
                return val;
            }
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
