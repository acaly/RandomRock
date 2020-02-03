using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions
{
    public interface IInterpolation
    {
        float Interpolate(float delta);
    }

    public class NearestInterpolation : IInterpolation
    {
        public float Interpolate(float delta)
        {
            return delta >= 0.5f ? 1 : 0;
        }
    }

    public class LinearInterpolation : IInterpolation
    {
        public float Interpolate(float delta)
        {
            return delta;
        }
    }

    public class HermiteInterpolation : IInterpolation
    {
        public float Interpolate(float delta)
        {
            return delta * delta * (3 - 2 * delta);
        }
    }

    public class QuinticInterpolation : IInterpolation
    {
        public float Interpolate(float delta)
        {
            var a = delta * delta;
            var b = delta * (delta * 6 - 15) + 10;
            return a * b * delta;
        }
    }
}
