using System;
using System.Collections.Generic;
using System.Linq;
using Scalesque;

namespace MonadicIT.Source.Lossless
{
    public class PrefixTree<T> : IPrefixTree where T : /* Enum, */ struct
    {
        private Either<T, Tuple<PrefixTree<T>, PrefixTree<T>>> _data;

        public double Probability { get; set; }
        public bool IsLeaf { get { return _data.IsLeft; } }
        public T Value { get { return _data.ProjectLeft().Get(); } }
        public PrefixTree<T> Left { get { return _data.ProjectRight().Get().Item1; } }
        public PrefixTree<T> Right { get { return _data.ProjectRight().Get().Item2; } }

        public IEnumerable<PrefixTree<T>> Children
        {
            get { return IsLeaf ? Enumerable.Empty<PrefixTree<T>>() : new[] {Left, Right}; }
        }

        private PrefixTree()
        {
        }

        public static PrefixTree<T> Leaf(double probability, T value)
        {
            return new PrefixTree<T>
            {
                Probability = probability,
                _data = Either.Left(value)
            };
        }

        public static PrefixTree<T> Inner(PrefixTree<T> l, PrefixTree<T> r)
        {
            return new PrefixTree<T>
            {
                Probability = l.Probability + r.Probability,
                _data = Either.Right(Tuple.Create(l, r))
            };
        }

        object IPrefixTree.Value
        {
            get { return Value; }
        }

        IPrefixTree IPrefixTree.Left
        {
            get { return Left; }
        }

        IPrefixTree IPrefixTree.Right
        {
            get { return Right; }
        }

        IEnumerable<IPrefixTree> IPrefixTree.Children
        {
            get { return Children; }
        }
    }
}