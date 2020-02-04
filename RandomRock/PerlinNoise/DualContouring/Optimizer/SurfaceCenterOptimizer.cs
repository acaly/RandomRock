using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.DualContouring.Optimizer
{
    class SurfaceCenterOptimizer : AbstractOptimizer
    {
        private int _n = 0;
        private Vector3 _p = new Vector3();

        public override void AddCrossEdge(Vector3 position, Vector3 gradient)
        {
            _n += 1;
            _p += position;
        }

        public override Vector3 Optimize()
        {
            return _p / _n;
        }

        public override void Reset()
        {
            _n = 0;
            _p = new Vector3();
        }
    }
}
