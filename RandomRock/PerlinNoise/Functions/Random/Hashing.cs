using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise.Functions.Random
{
    internal class Hashing
    {
        private const uint P = 0x01000193u;
        private const uint Z = 0x811C9DC5u;

        private static byte Fold(uint hash)
        {
            return (byte)(hash ^ (hash >> 8)); //TODO high 16 bit?
        }

        public static byte XorHash(int a, int b, int c)
        {
            uint hash = Z;
            hash ^= (uint)a;
            hash *= P;
            hash ^= (uint)b;
            hash *= P;
            hash ^= (uint)c;
            hash *= P;
            return Fold(hash);
        }

        public static byte XorHash(int a, int b, int c, int d)
        {
            uint hash = Z;
            hash ^= (uint)a;
            hash *= P;
            hash ^= (uint)b;
            hash *= P;
            hash ^= (uint)c;
            hash *= P;
            hash ^= (uint)d;
            hash *= P;
            return Fold(hash);
        }
    }
}
