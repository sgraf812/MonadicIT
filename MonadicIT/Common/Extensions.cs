﻿using System;
using System.Collections.Generic;
using MonadicIT.Channel;

namespace MonadicIT.Common
{
    public static class Extensions
    {
        public static Bool ToBool(this bool b)
        {
            return b ? Bool.True : Bool.False;
        }

        public static Distribution<V> SelectMany<T, U, V>(
            this Distribution<T> dist,
            Func<T, Distribution<U>> transitionDistribution,
            Func<T, U, V> jointSelector)
            where U : /* Enum, */ struct
            where T : /* Enum, */ struct
            where V : /* Enum, */ struct
        {
            return dist.MarginalDistribution(transitionDistribution, jointSelector);
        }

        public static double ErrorRate<T>(this IDiscreteChannel<T> channel)
            where T : /* Enum, */ struct
        {
            Distribution<Bool> errorDist = from a in Distribution<T>.Uniform(EnumHelper<T>.Values)
                                           from b in channel.GetTransitionDistribution(a)
                                           select a.Equals(b).ToBool();

            return errorDist[Bool.False];
        }

        public static IEnumerable<IList<T>> InChunksOf<T>(this IEnumerable<T> seq, int chunkSize)
        {
            var chunk = new List<T>(chunkSize);

            foreach (T x in seq)
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
            bool take = true;
            int n = 0;
            foreach (T x in seq)
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
            int n = 0;
            foreach (T x in seq)
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
// ReSharper disable once FunctionNeverReturns
        }

        public static IEnumerable<T> SingleValue<T>(this T value)
        {
            yield return value;
        }
    }
}