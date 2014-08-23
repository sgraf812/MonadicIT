using System;
using System.Linq;
using MonadicIT.Common;

namespace MonadicIT.Channel
{
    public class SymmetricChannel<T> : IDiscreteChannel<T> where T : /* Enum, */ struct
    {
        private readonly double _p;
        private readonly Distribution<T>[] _transitionMatrix;

        public SymmetricChannel(double correctTransmissionProbability)
        {
            EnumHelper<T>.ThrowUnlessEnum();

            int symbolCount = EnumHelper<T>.Values.Length;
            Throw.If<ArgumentException>(symbolCount < 2, "The channel must be have an arity " +
                                                         "of at least 2 to transmit information.");

            _p = correctTransmissionProbability;
            _transitionMatrix = CreateTransitionMatrix(_p);
        }

        public Distribution<T> GetTransitionDistribution(T symbol)
        {
            int idx = Array.IndexOf(EnumHelper<T>.Values, symbol);
            return _transitionMatrix[idx];
        }

        public double ChannelCapacity
        {
            get
            {
                int n = _transitionMatrix.Length;
                Func<double, double> log2 = x => Math.Log(x > 0 ? x : double.Epsilon, 2);
                return log2(n) + _p*log2(_p) + (1 - _p)*log2((1 - _p)/(n - 1));
            }
        }

        private static Distribution<T>[] CreateTransitionMatrix(double p)
        {
            T[] symbols = EnumHelper<T>.Values;
            int n = EnumHelper<T>.Values.Length;
            double pe = (1 - p)/(n - 1);

            Func<T, Distribution<T>> transition = a => Distribution<T>.FromProbabilites(
                from b in symbols
                select a.Equals(b)
                    ? Tuple.Create(b, p)
                    : Tuple.Create(b, pe));

            // now memoize each transition distribution
            return symbols.Select(transition).ToArray();
        }
    }
}