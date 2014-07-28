using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Scalesque;

namespace MonadicIT.Source.Lossless
{
    public class HuffmanCoder<T> : ISourceEncoder<T>, ISourceDecoder<T>
    {
        private readonly PrefixNode _decoder;
        private readonly IDictionary<T, IEnumerable<bool>> _encoder;

        private HuffmanCoder(PrefixNode decoder, IDictionary<T, IEnumerable<bool>> encoder)
        {
            _decoder = decoder;
            _encoder = encoder;
        }

        public IEnumerable<bool> Encode(IEnumerable<T> symbols)
        {
            return symbols.SelectMany(s => _encoder[s]);
        }

        public IEnumerable<T> Decode(IEnumerable<bool> bits)
        {
            var cur = _decoder;

            // iff we have only one code symbol, there is no information transmitted
            while (cur.IsLeaf) 
                yield return cur.Value; // infinite sequence of the single symbol

            foreach (var b in bits)
            {
                cur = b ? cur.Right : cur.Left;

                if (cur.IsLeaf)
                {
                    yield return cur.Value;
                    cur = _decoder;
                }
            }

            if (cur != _decoder) // the last symbol was decoded incompletely.
            {
                throw new InvalidDataException("The input bit stream ended unexpectedly. " +
                                               "There might be an incompletely decoded symbol.");
            }
        }

        public static HuffmanCoder<T> FromProbabilities(IDictionary<T, float> probabilities)
        {
            if (probabilities.Count == 0)
                throw new ArgumentException("There is no code for an empty dictionary");

            // intialize the priority queue with leafs corresponding to the probability map
            C5.IPriorityQueue<PrefixNode> queue = new C5.IntervalHeap<PrefixNode>(probabilities.Count, PrefixNode.Comparer);
            queue.AddAll(
                from p in probabilities
                select PrefixNode.Leaf(p.Value, p.Key));

            // the usual steps of the huffman algorithm
            var a = queue.DeleteMin();
            while (!queue.IsEmpty)
            {
                var b = queue.DeleteMin();
                queue.Add(PrefixNode.Inner(a, b));
                a = queue.DeleteMin();
            }

            // a is now the root of the code tree
            return new HuffmanCoder<T>(a, DictionaryFromPrefixTree(a));
        }

        private static IDictionary<T, IEnumerable<bool>> DictionaryFromPrefixTree(PrefixNode root)
        {
            var dict = new Dictionary<T, IEnumerable<bool>>();
            var path = new Stack<PrefixNode>();
            var bits = new Stack<bool>();
            path.Push(root);

            PrefixNode last = null;
            while (path.Count > 0)
            {
                var cur = path.Peek();

                if (cur.IsLeaf)
                {
                    dict[cur.Value] = bits.Reverse().ToArray();
                    path.Pop();
                    bits.Pop();
                }
                else
                {
                    if (last == cur.Left)
                    {
                        // visit the right child now
                        path.Push(cur.Right);
                        bits.Push(true); // 1
                    }
                    else if (last == cur.Right)
                    {
                        // cur is fully visited. ascend
                        path.Pop();
                        if (bits.Count > 0)
                        {
                            bits.Pop();
                        }
                        else
                        {
                            Debug.Assert(path.Count == 0, "Can only happen when the path turned empty.");
                        }
                    }
                    else
                    {
                        // visit the left child
                        path.Push(cur.Left);
                        bits.Push(false); // 0
                    }
                }

                last = cur;
            }

            return dict;
        }

        private class PrefixNode
        {
            public static readonly IComparer<PrefixNode> Comparer = new HuffmanNodeComparer();
            private float Probability { get; set; }
            private Either<T, Tuple<PrefixNode, PrefixNode>> _data;

            public bool IsLeaf { get { return _data.IsLeft; } }
            public T Value { get { return _data.ProjectLeft().Get(); } }
            public PrefixNode Left { get { return _data.ProjectRight().Get().Item1; } }
            public PrefixNode Right { get { return _data.ProjectRight().Get().Item2; } }

            private PrefixNode()
            {
            }

            public static PrefixNode Leaf(float probability, T value)
            {
                return new PrefixNode
                {
                    Probability = probability,
                    _data = Either.Left(value)
                };
            }

            public static PrefixNode Inner(PrefixNode l, PrefixNode r)
            {
                return new PrefixNode
                {
                    Probability = l.Probability + r.Probability,
                    _data = Either.Right(Tuple.Create(l, r))
                };
            }

            private class HuffmanNodeComparer : IComparer<PrefixNode>
            {
                public int Compare(PrefixNode x, PrefixNode y)
                {
                    var delta = x.Probability - y.Probability;
                    return delta < 0 ? -1 : delta > 0 ? 1 : 0;
                }
            }
        }
    }
}