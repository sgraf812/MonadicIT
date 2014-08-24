using System;
using System.Collections.Generic;
using System.Linq;

namespace MonadicIT.Common
{
    public class Distribution<T> : IDistribution where T : /* Enum, */ struct
    {
// ReSharper disable once StaticFieldInGenericType
        private static readonly Random Rng = new Random();
        private readonly Tuple<T, double>[] _probs;

        private Distribution(Tuple<T, double>[] probs)
        {
            _probs = probs;
        }

        public double this[T symbol]
        {
            get { return _probs.First(p => p.Item1.Equals(symbol)).Item2; }
        }

        public double Entropy
        {
            get
            {
                return (from prob in _probs
                        let p = prob.Item2
                        where p > 0
                        select -p*Math.Log(p, 2)).Sum();
            }
        }

        public Type SymbolType
        {
            get { return typeof (T); }
        }

        double IDistribution.this[object symbol]
        {
            get { return this[(T) symbol]; }
        }

        object IDistribution.Sample()
        {
            return Sample();
        }

        public T Sample()
        {
            double rnd = Rng.NextDouble();
            double accum = 0.0;
            foreach (var t in _probs)
            {
                double p = t.Item2;
                accum += p;
                if (accum >= rnd)
                {
                    return t.Item1;
                }
            }
            // can not reach this point
            return default(T);
        }

        public Distribution<U> JointDistribution<U>(Func<T, Distribution<U>> transitionDistribution)
            where U : /* Enum, */ struct
        {
            IEnumerable<Tuple<U, double>> probs =
                from t in _probs
                from u in transitionDistribution(t.Item1)._probs
                group new {t, u} by u.Item1
                into g
                select Tuple.Create(g.Key, g.Sum(x => x.t.Item2*x.u.Item2));

            return Distribution<U>.FromProbabilites(probs);
        }

        public Distribution<V> JointDistribution<U, V>(Func<T, Distribution<U>> transitionDistribution,
            Func<T, U, V> jointSelector)
            where U : /* Enum, */ struct
            where V : /* Enum, */ struct
        {
            IEnumerable<Tuple<V, double>> probs =
                from t in _probs
                from u in transitionDistribution(t.Item1)._probs
                group new {t, u} by jointSelector(t.Item1, u.Item1)
                into g
                select Tuple.Create(g.Key, g.Sum(x => x.t.Item2*x.u.Item2));

            return Distribution<V>.FromProbabilites(probs);
        }

        public static Distribution<T> FromProbabilites(IEnumerable<Tuple<T, double>> probabilities)
        {
            EnumHelper<T>.ThrowUnlessEnum();

            Tuple<T, double>[] probs = probabilities.ToArray();

            Throw.If<ArgumentException>(Math.Abs(probs.Sum(p => p.Item2) - 1.0) > 0.0001,
                "The probabilites must sum up to 1.");

            IEnumerable<T> mentionedSymbols = from p in probs
                                              select p.Item1;
            IEnumerable<T> notMentioned = EnumHelper<T>.Values.Except(mentionedSymbols);
            IEnumerable<Tuple<T, double>> allSymbols = probs.Concat(from s in notMentioned
                                                                    select Tuple.Create(s, 0.0));

            return new Distribution<T>(allSymbols.ToArray());
        }

        public static Distribution<T> Uniform(IEnumerable<T> values)
        {
            T[] vals = values.ToArray();
            IEnumerable<Tuple<T, double>> probs = from v in vals
                                                  select Tuple.Create(v, 1.0/vals.Length);

            return FromProbabilites(probs);
        }

        public static Distribution<T> Certainly(T value)
        {
            return FromProbabilites(new[] {Tuple.Create(value, 1.0)});
        }
    }
}