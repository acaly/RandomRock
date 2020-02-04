using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.DualContouring.Optimizer
{
    abstract class AbstractOptimizer
    {
        public abstract void Reset();
        public abstract void AddCrossEdge(Vector3 position, Vector3 gradient);
        public abstract Vector3 Optimize();
    }
}
