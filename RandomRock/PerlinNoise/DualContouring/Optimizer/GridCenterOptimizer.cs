using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.DualContouring.Optimizer
{
    class GridCenterOptimizer : AbstractOptimizer
    {
        public override void AddCrossEdge(Vector3 position, Vector3 gradient)
        {
        }

        public override Vector3 Optimize()
        {
            return new Vector3(0.5f, 0.5f, 0.5f);
        }

        public override void Reset()
        {
        }
    }
}
