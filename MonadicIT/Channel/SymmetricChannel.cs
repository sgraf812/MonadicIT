using System;
using System.Linq;
using MonadicIT.Common;

namespace MonadicIT.Channel
{
    public class SymmetricChannel<T> : IDiscreteChannel<T> where T : /* Enum, */ struct
    {
        private readonly Distribution<T>[] _transitionMatrix;
        private readonly double _p;

        public SymmetricChannel(double correctTransmissionProbability)
        {
            EnumHelper<T>.ThrowUnlessEnum();

            var symbolCount = EnumHelper<T>.Values.Length;
            Throw.If<ArgumentException>(symbolCount < 2, "The channel must be have an arity " +
                                                         "of at least 2 to transmit information.");

            _p = correctTransmissionProbability;
            _transitionMatrix = CreateTransitionMatrix(_p);
        }

        private static Distribution<T>[] CreateTransitionMatrix(double p)
        {
            var symbols = EnumHelper<T>.Values;
            var n = EnumHelper<T>.Values.Length;
            var pe = (1 - p)/(n - 1);

            Func<T, Distribution<T>> transition = a => Distribution<T>.FromProbabilites(
                from b in symbols
                select a.Equals(b)
                    ? Tuple.Create(b, p)
                    : Tuple.Create(b, pe));

            // now memoize each transition distribution
            return symbols.Select(transition).ToArray();
        } 

        public Distribution<T> GetTransitionDistribution(T symbol)
        {
            var idx = Array.IndexOf(EnumHelper<T>.Values, symbol);
            return _transitionMatrix[idx];
        }

        public double ChannelCapacity
        {
            get
            {
                var n = _transitionMatrix.Length;
                Func<double, double> log2 = x => Math.Log(x, 2);
                return log2(n) + _p*log2(_p) + (1 - _p)*log2((1 - _p)/(n - 1));
            }
        }
    }
}