using System;
using System.Collections.Generic;
using System.Linq;

namespace MonadicIT.Common
{
    public class Distribution<T> where T : /* Enum, */ struct
    {
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
            get { return _probs.Sum(p => -p.Item2 * Math.Log(p.Item2, 2)); }
        }

        public Distribution<U> SelectMany<U>(Func<T, Distribution<U>> transitionDistribution)
            where U : /* Enum, */ struct
        {
            var probs =
                from t in _probs
                from u in transitionDistribution(t.Item1)._probs
                group new {t, u} by u.Item1
                into g
                select Tuple.Create(g.Key, g.Sum(x => x.t.Item2*x.u.Item2));

            return Distribution<U>.FromProbabilites(probs);
        } 

        public static Distribution<T> FromProbabilites(IEnumerable<Tuple<T, double>> probabilities)
        {
            EnumHelper<T>.ThrowUnlessEnum();

            var probs = probabilities.ToArray();

            Throw.If<ArgumentException>(Math.Abs(probs.Sum(p => p.Item2) - 1.0) > 0.0001,
                "The probabilites must sum up to 1.");

            var mentionedSymbols = from p in probs 
                                   select p.Item1;
            var notMentioned = EnumHelper<T>.Values.Except(mentionedSymbols);
            var allSymbols = probs.Concat(from s in notMentioned 
                                          select Tuple.Create(s, 0.0));

            return new Distribution<T>(allSymbols.ToArray());
        }

        public static Distribution<T> Uniform(IEnumerable<T> values)
        {
            var vals = values.ToArray();
            var probs = from v in vals
                        select Tuple.Create(v, 1.0/vals.Length);

            return FromProbabilites(probs);
        }

        public static Distribution<T> Certainly(T value)
        {
            return FromProbabilites(new[] {Tuple.Create(value, 1.0)});
        } 
    }
}