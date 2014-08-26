using System;
using System.Collections.Generic;
using System.Linq;

namespace MonadicIT.Common
{
    public class Distribution<T> : IDistribution where T : /* Enum, */ struct
    {
// ReSharper disable once StaticFieldInGenericType
        private static readonly Random Rng = new Random();
        private readonly SymbolProbability[] _probs;

        private Distribution(SymbolProbability[] probs)
        {
            _probs = probs;
        }

        public double this[T symbol]
        {
            get { return _probs.First(p => p.Symbol.Equals(symbol)).Probability; }
        }

        public double Entropy
        {
            get
            {
                return (from prob in _probs
                        let p = prob.Probability
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
                double p = t.Probability;
                accum += p;
                if (accum >= rnd)
                {
                    return t.Symbol;
                }
            }
            // can not reach this point
            return default(T);
        }

        public Distribution<V> MarginalDistribution<U, V>(Func<T, Distribution<U>> transitionDistribution,
            Func<T, U, V> jointSelector)
            where U : /* Enum, */ struct
            where V : /* Enum, */ struct
        {
            IEnumerable<Tuple<V, double>> probs =
                from t in _probs
                from u in transitionDistribution(t.Symbol)._probs
                select Tuple.Create(jointSelector(t.Symbol, u.Symbol), t.Probability*u.Probability);

            IEnumerable<Tuple<V, double>> normalizedProbs =
                from v in probs
                group v.Item2 by v.Item1
                into g
                select Tuple.Create(g.Key, g.Sum());

            return Distribution<V>.FromProbabilites(normalizedProbs);
        }

        public static Distribution<T> FromProbabilites(IEnumerable<Tuple<T, double>> probabilities)
        {
            EnumHelper<T>.ThrowUnlessEnum();

            SymbolProbability[] probs = probabilities
                .Select(p => new SymbolProbability {Symbol = p.Item1, Probability = p.Item2})
                .ToArray();

            Throw.If<ArgumentException>(Math.Abs(probs.Sum(p => p.Probability) - 1.0) > 0.0001,
                "The probabilites must sum up to 1.");

            IEnumerable<T> mentionedSymbols = from p in probs
                                              select p.Symbol;
            IEnumerable<T> notMentioned = EnumHelper<T>.Values.Except(mentionedSymbols);
            IEnumerable<SymbolProbability> allSymbols = probs.Concat(from s in notMentioned
                                                                     select new SymbolProbability {Symbol = s, Probability = 0.0});

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

        private struct SymbolProbability
        {
            public T Symbol { get; set; }
            public double Probability { get; set; }
        }
    }
}