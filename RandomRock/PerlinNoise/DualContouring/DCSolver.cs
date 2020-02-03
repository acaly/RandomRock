using PerlinNoise.Functions;
using PerlinNoise.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.DualContouring
{
    //A Dual contouring algorithm implementation explained by the following link:
    //http://www.boristhebrave.com/2018/04/15/dual-contouring-tutorial/
    //Currently unfinished (choosing the optimal position in cell), but this actually works well already.
    public class DCSolver
    {
        private readonly int _size;
        private readonly GridCache _cache;
        private readonly ModelStorage _model;

        private struct QueuedEdgeInfo
        {
            public GridEdge Edge;
            public bool ReversedNormal;
        }

        private HashSet<long> _queuedEdgeSet = new HashSet<long>();
        private Queue<QueuedEdgeInfo> _queuedEdge = new Queue<QueuedEdgeInfo>();

        public DCSolver(IFunction function, int size)
        {
            _size = size;
            _cache = new GridCache(function, size);
            _model = new ModelStorage(size);
        }

        public RawModel Solve()
        {
            Run();
            return _model.Generate();
        }

        private GridEdge FindFirst()
        {
            GridCoordinate g0 = new GridCoordinate(0, 0, 0);
            GridCoordinate gMove = new GridCoordinate(1, 0, 0);
            if (_cache.EvaluateAtGrid(g0, GridCoordinate.Zero) < 0)
            {
                throw new Exception("Origin not contained in shape");
            }
            var g1 = g0 + gMove;
            while (_cache.EvaluateAtGrid(g1, GridCoordinate.Zero) > 0)
            {
                g0 = g1;
                g1 += gMove;
            }
            return new GridEdge(g0, 0);
        }

        private void EnqueueEdge(GridEdge edge, bool reversedNormal)
        {
            _queuedEdge.Enqueue(new QueuedEdgeInfo { Edge = edge, ReversedNormal = reversedNormal });
            _queuedEdgeSet.Add(edge.GetEdgeLong(_size));
        }

        private void Run()
        {
            EnqueueEdge(FindFirst(), false);
            while (_queuedEdge.Count > 0)
            {
                RunIteration();
            }
        }

        //Same as ModelStorage._axisGridOffsetList
        private GridCoordinate[] _edgeCoordToAdjacentGrid = new[]
        {
            new GridCoordinate(0, -1, -1), new GridCoordinate(0, 0, -1), new GridCoordinate(0, -1, 0),
            new GridCoordinate(-1, 0, -1), new GridCoordinate(0, 0, -1), new GridCoordinate(-1, 0, 0),
            new GridCoordinate(-1, -1, 0), new GridCoordinate(0, -1, 0), new GridCoordinate(-1, 0, 0),
        };

        private void RunIteration()
        {
            var e = _queuedEdge.Dequeue();
            
            PrepareGridPoint(e.Edge.Coord);
            for (var i = 0; i < 3; ++i)
            {
                PrepareGridPoint(e.Edge.Coord + _edgeCoordToAdjacentGrid[i + e.Edge.Axis * 3]);
            }

            _model.AddFace(e.Edge, e.ReversedNormal);
            EnqueueAdjacent(e.Edge);
        }

        private GridCoordinate[] _findAdjacentEdgeList = new[]
        {
            //==== x edge ====
            // -> x edge
            new GridCoordinate(0, -1, 0), new GridCoordinate(0, 1, 0), new GridCoordinate(0, 0, -1), new GridCoordinate(0, 0, 1),
            // -> y edge
            new GridCoordinate(0, -1, 0), new GridCoordinate(0, 0, 0), new GridCoordinate(1, -1, 0), new GridCoordinate(1, 0, 0),
            // -> z edge
            new GridCoordinate(0, 0, -1), new GridCoordinate(0, 0, 0), new GridCoordinate(1, 0, -1), new GridCoordinate(1, 0, 0),
            //==== y edge ====
            // -> x edge
            new GridCoordinate(-1, 0, 0), new GridCoordinate(0, 0, 0), new GridCoordinate(-1, 1, 0), new GridCoordinate(0, 1, 0),
            // -> y edge
            new GridCoordinate(-1, 0, 0), new GridCoordinate(1, 0, 0), new GridCoordinate(0, 0, -1), new GridCoordinate(0, 0, 1),
            // -> z edge
            new GridCoordinate(0, 0, -1), new GridCoordinate(0, 0, 0), new GridCoordinate(0, 1, -1), new GridCoordinate(0, 1, 0),
            //==== z edge ====
            // -> x edge
            new GridCoordinate(-1, 0, 0), new GridCoordinate(0, 0, 0), new GridCoordinate(-1, 0, 1), new GridCoordinate(0, 0, 1),
            // -> y edge
            new GridCoordinate(0, -1, 0), new GridCoordinate(0, 0, 0), new GridCoordinate(0, -1, 1), new GridCoordinate(0, 0, 1),
            // -> z edge
            new GridCoordinate(-1, 0, 0), new GridCoordinate(1, 0, 0), new GridCoordinate(0, -1, 0), new GridCoordinate(0, 1, 0),
        };

        private void EnqueueAdjacent(GridEdge e)
        {
            for (var axis = 0; axis < 3; ++axis)
            {
                for (var i = 0; i < 4; ++i)
                {
                    GridEdge newEdge = new GridEdge(e.Coord + _findAdjacentEdgeList[e.Axis * 12 + axis * 4 + i], axis);
                    if (_queuedEdgeSet.Contains(newEdge.GetEdgeLong(_size)))
                    {
                        continue;
                    }
                    var lowVal = _cache.EvaluateAtGrid(newEdge.Low, GridCoordinate.Zero);
                    var highVal = _cache.EvaluateAtGrid(newEdge.High, GridCoordinate.Zero);
                    if (lowVal > 0 && highVal < 0)
                    {
                        EnqueueEdge(newEdge, false);
                    }
                    else if (lowVal < 0 && highVal > 0)
                    {
                        EnqueueEdge(newEdge, true);
                    }
                }
            }
        }

        private void PrepareGridPoint(GridCoordinate grid)
        {
            //Select grid point
            //TODO do real calculation
            //_model.AddVertex(grid.ToVector() + new Vector3(0.5f, 0.5f, 0.5f));
            _model.AddVertex(_cache.CalculateOptimalPoint(grid));
        }
    }
}
