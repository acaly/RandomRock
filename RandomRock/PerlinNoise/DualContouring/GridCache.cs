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

        private float[] _qfCoefficient = new float[10];

        private const float _gridVertexCenterBias = .1f;
        private static readonly float[] _qfCoefficient0 = new float[]
        {
            _gridVertexCenterBias, _gridVertexCenterBias, _gridVertexCenterBias,
            0, 0, 0,
            -2 * _gridVertexCenterBias, -2 * _gridVertexCenterBias, -2 * _gridVertexCenterBias,
            0,
        };

        //r: reference point. g: gradient at (or near) r
        private static void AddCoefficient(float[] coefficient, Vector3 r, Vector3 g)
        {
            //dot(p - r, g) ^ 2
            //[ (x - rx) * gx + (y - ry) * gy + (z - rz) * gz ] ^ 2
            //[ gx * x + gy * y + gz * z - rx * gx - ry * gy - rz * gz ] ^2
            //[ A x + B y + C z + D] ^ 2
            var A = g.X;
            var B = g.Y;
            var C = g.Z;
            var D = -Vector3.Dot(g, r);
            coefficient[0] += A * A; //x^2
            coefficient[1] += B * B; //y*2
            coefficient[2] += C * C; //z*2
            coefficient[3] += 2 * A * B; //xy
            coefficient[4] += 2 * B * C; //yz
            coefficient[5] += 2 * C * A; //zx
            coefficient[6] += 2 * A * D; //x
            coefficient[7] += 2 * B * D; //y
            coefficient[8] += 2 * C * D; //z
            coefficient[9] += D * D; //1
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

        private static Vector3 SolveQFSingle(float[] coefficient)
        {
            //https://www.value-at-risk.net/minimizing-a-quadratic-polynomial/
            //Minimize x' c x + b x + a
            //c:
            //x^2 xy  xz
            //    y^2 yz
            //        z^2
            Matrix4x4 c = new Matrix4x4(
                coefficient[0], coefficient[3], coefficient[5], 0,
                0, coefficient[1], coefficient[4], 0,
                0, 0, coefficient[2], 0,
                0, 0, 0, 1);
            if (!Matrix4x4.Invert(c, out var ic))
            {
                throw new Exception("Internal error");
            }
            var b = new Vector4(coefficient[6], coefficient[7], coefficient[8], 0);

            var ret = Vector4.Transform(b, ic) * -0.5f;
            return new Vector3(ret.X, ret.Y, ret.Z);
        }

        //For x, xx xy xz x y z
        private static readonly int[][] _coefficientAxisMap = new[]
        {
            new [] { 0, 3, 5, 6, 7, 8 },
            new [] { 1, 4, 3, 7, 8, 6 },
            new [] { 2, 5, 4, 8, 6, 7 },
        };

        private static float[] ApplyPosition(float[] coefficient, float val, int axis)
        {
            var index = _coefficientAxisMap[axis];

            var ret = new float[coefficient.Length];
            Array.Copy(coefficient, 0, ret, 0, ret.Length);

            //xy xz -> y z
            ret[index[4]] += coefficient[index[1]] * val;
            ret[index[5]] += coefficient[index[2]] * val;

            //clear xy xz
            ret[index[1]] = 0;
            ret[index[2]] = 0;

            //add x^2-2x (let x=1)
            ret[index[0]] = 1;
            ret[index[3]] = -2;

            return ret;
        }

        private static float EvaluateQF(float[] coefficient, Vector3 p)
        {
            return coefficient[0] * p.X * p.X +
                coefficient[1] * p.Y * p.Y +
                coefficient[2] * p.Z * p.Z +
                coefficient[3] * p.X * p.Y +
                coefficient[4] * p.Y * p.Z +
                coefficient[5] * p.Z * p.X +
                coefficient[6] * p.X +
                coefficient[7] * p.Y +
                coefficient[8] * p.Z;
        }

        private static Vector3 SolveQF(float[] coefficient)
        {
            const float constraint = 0.7f;
            const float constraintMax = 1 + constraint, constraintMin = 1 - constraint;

            var p = SolveQFSingle(coefficient);
            var dx = Math.Abs(p.X - 1);
            var dy = Math.Abs(p.Y - 1);
            var dz = Math.Abs(p.Z - 1);
            
            var maxd = Math.Max(dx, Math.Max(dy, dz));
            if (maxd <= constraint) return p;

            Vector3 ret = new Vector3();
            float val = float.PositiveInfinity;

            if (dx > constraint)
            {
                var nco = ApplyPosition(coefficient, p.X > 1 ? constraintMax : constraintMin, 0);
                var np = SolveQF(nco);
                np.X = p.X > 1 ? constraintMax : constraintMin;
                var nval = EvaluateQF(coefficient, np);
                if (val > nval)
                {
                    ret = np;
                    val = nval;
                }
            }
            if (dy > constraint)
            {
                var nco = ApplyPosition(coefficient, p.Y > 1 ? constraintMax : constraintMin, 1);
                var np = SolveQF(nco);
                np.Y = p.Y > 1 ? constraintMax : constraintMin;
                var nval = EvaluateQF(coefficient, np);
                if (val > nval)
                {
                    ret = np;
                    val = nval;
                }
            }
            if (dz > constraint)
            {
                var nco = ApplyPosition(coefficient, p.Z > 1 ? constraintMax : constraintMin, 2);
                var np = SolveQF(nco);
                np.Z = p.Z > 1 ? constraintMax : constraintMin;
                var nval = EvaluateQF(coefficient, np);
                if (val > nval)
                {
                    ret = np;
                    val = nval;
                }
            }
            return ret;
        }

        public Vector3 CalculateOptimalPoint(GridCoordinate grid)
        {
            if (_optimalPoint.TryGetValue(grid.GetGridLong(_size), out var ret))
            {
                return ret;
            }

            var internalGrid = grid + grid;
            //Array.Clear(_qfCoefficient, 0, _qfCoefficient.Length);
            Array.Copy(_qfCoefficient0, 0, _qfCoefficient, 0, _qfCoefficient.Length);

            var nCrossPoint = 0;
            var sumCrossPoint = new Vector3();

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
                        AddCoefficient(_qfCoefficient, r, g);

                        nCrossPoint += 1;
                        sumCrossPoint += r;
                    }
                }
            }

            return grid.ToVector() + sumCrossPoint / nCrossPoint / 2;

            //The solver does not give better results than simply taking the center.

            var xx = SolveQF(_qfCoefficient) * 0.5f; //Convert to normal grid.

            ////If outside range, choose the nearest point.
            //var x = Math.Max(0.1f, Math.Min(0.9f, xx.X));
            //var y = Math.Max(0.1f, Math.Min(0.9f, xx.Y));
            //var z = Math.Max(0.1f, Math.Min(0.9f, xx.Z));
            var x = Math.Abs(xx.X - 0.5f) < 0.5f ? xx.X : 0.5f;
            var y = Math.Abs(xx.Y - 0.5f) < 0.5f ? xx.Y : 0.5f;
            var z = Math.Abs(xx.Z - 0.5f) < 0.5f ? xx.Z : 0.5f;
            ret = grid.ToVector() + new Vector3(x, y, z);

            _optimalPoint.Add(grid.GetGridLong(_size), ret);
            return ret;
        }
    }
}
