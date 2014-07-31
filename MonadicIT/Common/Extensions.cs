using System;
using System.Collections;
using System.Collections.Generic;

namespace MonadicIT.Common
{
    public static class Extensions
    {
        public static Func<A, C> Then<A, B, C>(this Func<A, B> f, Func<B, C> g)
        {
            return a => g(f(a));
        }

        public static IEnumerable<IReadOnlyList<T>> InChunksOf<T>(this IEnumerable<T> seq, int chunkSize)
        {
            var chunk = new List<T>(chunkSize);

            foreach (var x in seq)
            {
                chunk.Add(x);

                if (chunk.Count < chunkSize) continue;

                yield return chunk;
                chunk = new List<T>(chunkSize);
            }

            if (chunk.Count > 0)
            {
                yield return chunk;
            }
        }

        public static IEnumerable<T> AlternatingTakeAndSkip<T>(this IEnumerable<T> seq, int chunkSize)
        {
            var take = true;
            var n = 0;
            foreach (var x in seq)
            {
                n++;
                if (take)
                {
                    yield return x;
                }

                if (n >= chunkSize)
                {
                    take = !take;
                    n = 0;
                }
            }
        }

        public static IEnumerable<T> PadWith<T>(this IEnumerable<T> seq, T value, int desiredSize)
        {
            var n = 0;
            foreach (var x in seq)
            {
                n++;
                yield return x;
            }

            while (n < desiredSize)
            {
                n++;
                yield return value;
            }
        }

        public static IEnumerable<T> Repeat<T>(this T value)
        {
            while (true)
            {
                yield return value;
            }
        }
    }
}