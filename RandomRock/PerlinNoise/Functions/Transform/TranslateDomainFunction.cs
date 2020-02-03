using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions.Transform
{
    public class TranslateDomainFunction : IFunction
    {
        public IFunction Input { get; set; }
        public IFunction TranslationX { get; set; }
        public IFunction TranslationY { get; set; }
        public IFunction TranslationZ { get; set; }

        public float Get(uint? seed, Vector3 coord)
        {
            Vector3 tr = new Vector3();
            tr.X = TranslationX?.Get(null, coord) ?? 0f;
            tr.Y = TranslationY?.Get(null, coord) ?? 0f;
            tr.Z = TranslationZ?.Get(null, coord) ?? 0f;

            return Input.Get(null, coord + tr);
        }
    }
}
