using System;

namespace Unity.Multiplayer.Tools.Util
{
    internal static class RingBufferExtensions
    {
        // Unfortunately in C# it's not possible to implement these Sum and Average extensions
        // generically without imposing additional overhead, as it would be with C++ templates

        public static int Sum(this RingBuffer<int> ring)
        {
            int sum = 0;
            var count = ring.Length;
            for (var i = 0; i < count; ++i)
            {
                sum += ring[i];
            }
            return sum;
        }

        public static long Sum(this RingBuffer<long> ring)
        {
            long sum = 0;
            var count = ring.Length;
            for (var i = 0; i < count; ++i)
            {
                sum += ring[i];
            }
            return sum;
        }

        public static float Sum(this RingBuffer<float> ring)
        {
            float sum = 0;
            var count = ring.Length;
            for (var i = 0; i < count; ++i)
            {
                sum += ring[i];
            }
            return sum;
        }

        public static int SumLastN(this RingBuffer<int> ring, int n)
        {
            int sum = 0;
            var count = ring.Length;
            var first = count - n;
            for (var i = first; i < count; ++i)
            {
                sum += ring[i];
            }
            return sum;
        }

        public static long SumLastN(this RingBuffer<long> ring, int n)
        {
            long sum = 0;
            var count = ring.Length;
            var first = count - n;
            for (var i = first; i < count; ++i)
            {
                sum += ring[i];
            }
            return sum;
        }

        public static float SumLastN(this RingBuffer<float> ring, int n)
        {
            float sum = 0;
            var count = ring.Length;
            var first = count - n;
            for (var i = first; i < count; ++i)
            {
                sum += ring[i];
            }
            return sum;
        }

        public static float Average(this RingBuffer<int> ring)
        {
            var sum = ring.Sum();
            return (float)sum / ring.Length;
        }

        public static float Average(this RingBuffer<long> ring)
        {
            var sum = ring.Sum();
            return (float)sum / ring.Length;
        }

        public static float Average(this RingBuffer<float> ring)
        {
            var sum = ring.Sum();
            return sum / ring.Length;
        }
    }
}