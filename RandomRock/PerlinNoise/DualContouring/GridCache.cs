using PerlinNoise.DualContouring.Optimizer;
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

        //===== function value cache =====
        //Note that we use internal grid here.
        private struct GridInfo { public float Val; public long Time; }
        private readonly SortedDictionary<long, long> _timeGrid = new SortedDictionary<long, long>();
        private readonly Dictionary<long, GridInfo> _gridValue = new Dictionary<long, GridInfo>();
        private long _nextTime = 0;

        //===== optimal point cache =====
        //Here grid is normal scale.
        private readonly Dictionary<long, Vector3> _optimalPoint = new Dictionary<long, Vector3>();

        private readonly AbstractOptimizer _optimizer = new SurfaceCenterOptimizer();

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

        //Note that the value cache at grid vertices are internal grid (1/2 width).
        //This allows caching values at edge and face center points.
        private float EvaluateAtInternalGrid(GridCoordinate coord)
        {
            var g = coord.GetGridLong(_size);
            var tryRead = ReadCache(g);
            if (tryRead.HasValue)
            {
                return tryRead.Value;
            }
            else
            {
                var val = _function.Get(null, coord.ToVector() / 2);
                AddToCache(g, val);
                return val;
            }
        }

        public float EvaluateAtGrid(GridCoordinate coord, GridCoordinate halfOffset)
        {
            return EvaluateAtInternalGrid(coord + coord + halfOffset);
        }

        public GridCache(IFunction function, int maxSize)
        {
            _function = function;
            _size = maxSize;
            _cacheCount = maxSize * 40 + 20;
        }

        private float EvaluateDifference(GridCoordinate internalGrid, GridCoordinate diff)
        {
            return EvaluateAtInternalGrid(internalGrid + diff) - EvaluateAtInternalGrid(internalGrid - diff);
        }

        private Vector3 EvaluateGradient(GridCoordinate internalGrid)
        {
            var x = EvaluateDifference(internalGrid, new GridCoordinate(1, 0, 0));
            var y = EvaluateDifference(internalGrid, new GridCoordinate(0, 1, 0));
            var z = EvaluateDifference(internalGrid, new GridCoordinate(0, 0, 1));
            return new Vector3(x, y, z);
        }

        private static readonly GridCoordinate[] _gridEdgeList = new[]
        {
            //x
            new GridCoordinate(0, 0, 0), new GridCoordinate(0, 2, 0), new GridCoordinate(0, 0, 2), new GridCoordinate(0, 2, 2),
            //y
            new GridCoordinate(0, 0, 0), new GridCoordinate(0, 0, 2), new GridCoordinate(2, 0, 0), new GridCoordinate(2, 0, 2),
            //z
            new GridCoordinate(0, 0, 0), new GridCoordinate(2, 0, 0), new GridCoordinate(0, 2, 0), new GridCoordinate(2, 2, 0),
        };

        //TODO should move to GridCoordinate
        private static readonly GridCoordinate[] _axisIncList = new[]
        {
            new GridCoordinate(1, 0, 0), new GridCoordinate(0, 1, 0), new GridCoordinate(0, 0, 1),
        };

        public Vector3 CalculateOptimalPoint(GridCoordinate grid)
        {
            if (_optimalPoint.TryGetValue(grid.GetGridLong(_size), out var ret))
            {
                return ret;
            }

            var internalGrid = grid + grid;
            _optimizer.Reset();

            for (var axis = 0; axis < 3; ++axis)
            {
                var edgeHalfDiff = _axisIncList[axis];
                var edgeFullDiff = edgeHalfDiff + edgeHalfDiff;
                for (var i = 0; i < 4; ++i)
                {
                    var low = internalGrid + _gridEdgeList[axis * 4 + i];
                    var high = low + edgeFullDiff;
                    var lowVal = EvaluateAtInternalGrid(low);
                    var highVal = EvaluateAtInternalGrid(high);
                    if (Math.Sign(lowVal) != Math.Sign(highVal))
                    {
                        var pos = (0 - lowVal) / (highVal - lowVal);
                        //TODO simplify
                        var r = pos * high.ToVector() + (1 - pos) * low.ToVector() - internalGrid.ToVector();
                        var g = Vector3.Normalize(EvaluateGradient(low + edgeHalfDiff));

                        _optimizer.AddCrossEdge(r / 2, g);
                    }
                }
            }

            ret = grid.ToVector() + _optimizer.Optimize();
            _optimalPoint.Add(grid.GetGridLong(_size), ret);
            return ret;
        }
    }
}
