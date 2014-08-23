using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using C5;
using MonadicIT.Common;

namespace MonadicIT.Source.Lossless
{
    public class HuffmanCoder<T> : ISourceEncoder<T>, ISourceDecoder<T> where T : /* Enum, */ struct
    {
        private static readonly IComparer<PrefixTree<T>> Comparer = new HuffmanNodeComparer();

        private HuffmanCoder(PrefixTree<T> codeTree, Dictionary<T, IEnumerable<Binary>> codeDictionary)
        {
            CodeTree = codeTree;
            CodeDictionary = codeDictionary;
        }

        public Dictionary<T, IEnumerable<Binary>> CodeDictionary { get; private set; }
        public PrefixTree<T> CodeTree { get; private set; }

        public IEnumerable<T> Decode(IEnumerable<Binary> bits)
        {
            PrefixTree<T> cur = CodeTree;

            // iff we have only one code symbol, there is no information transmitted
            while (cur.IsLeaf)
                yield return cur.Value; // infinite sequence of the single symbol

            foreach (Binary b in bits)
            {
                cur = b == Binary.I ? cur.Right : cur.Left;

                if (cur.IsLeaf)
                {
                    yield return cur.Value;
                    cur = CodeTree;
                }
            }

            Throw.If<InvalidDataException>(cur != CodeTree, "The input bit stream ended unexpectedly. " +
                                                            "There might be an incompletely decoded symbol.");
        }

        public IEnumerable<Binary> Encode(IEnumerable<T> symbols)
        {
            return symbols.SelectMany(s => CodeDictionary[s]);
        }

        public Distribution<Binary> GetBitDistribution(Distribution<T> distribution)
        {
            Tuple<double, double>[] weightedOneAndZeros = (from s in EnumHelper<T>.Values
                                                           let p = distribution[s]
                                                           where p > 0
                                                           let bits = Encode(new[] {s}).ToArray()
                                                           let zeros = bits.Count(b => b == Binary.O)
                                                           let ones = bits.Count(b => b == Binary.I)
                                                           let all = bits.Length
                                                           select Tuple.Create(p*zeros/all, p*ones/all)).ToArray();
            double pZero = weightedOneAndZeros.Sum(t => t.Item1);
            double pOne = weightedOneAndZeros.Sum(t => t.Item2);

            return
                Distribution<Binary>.FromProbabilites(new[]
                {Tuple.Create(Binary.O, pZero), Tuple.Create(Binary.I, pOne)});
        }

        public static HuffmanCoder<T> FromDistribution(Distribution<T> distribution)
        {
            int symbolCount = EnumHelper<T>.Values.Length;
            Throw.If<ArgumentException>(symbolCount == 0,
                "There is no code for an empty dictionary");

            // intialize the priority queue with leafs corresponding to the probability map
            IPriorityQueue<PrefixTree<T>> queue = new IntervalHeap<PrefixTree<T>>(symbolCount, Comparer);
            queue.AddAll(
                from s in EnumHelper<T>.Values
                let p = distribution[s]
                where p > 0
                select PrefixTree<T>.Leaf(p, s));

            // the usual steps of the huffman algorithm
            PrefixTree<T> a = queue.DeleteMin();
            while (!queue.IsEmpty)
            {
                PrefixTree<T> b = queue.DeleteMin();
                queue.Add(PrefixTree<T>.Inner(a, b));
                a = queue.DeleteMin();
            }

            // a is now the root of the code tree
            return new HuffmanCoder<T>(a, DictionaryFromPrefixTree(a));
        }

        private static Dictionary<T, IEnumerable<Binary>> DictionaryFromPrefixTree(PrefixTree<T> root)
        {
            var dict = new Dictionary<T, IEnumerable<Binary>>();
            var path = new Stack<PrefixTree<T>>();
            var bits = new Stack<Binary>();
            path.Push(root);

            PrefixTree<T> last = null;
            while (path.Count > 0)
            {
                PrefixTree<T> cur = path.Peek();

                if (cur.IsLeaf)
                {
                    dict[cur.Value] = bits.Reverse().ToArray();
                    path.Pop();
                    if (bits.Count == 0)
                    {
                        // does only happen when there is exactly one symbol with probability 1.
                        // in that case, the path will be emtpy, too.
                        Debug.Assert(path.Count == 0);
                    }
                    else
                    {
                        bits.Pop();
                    }
                }
                else
                {
                    if (last == cur.Left)
                    {
                        // visit the right child now
                        path.Push(cur.Right);
                        bits.Push(Binary.I);
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
                        bits.Push(Binary.O);
                    }
                }

                last = cur;
            }

            return dict;
        }

        private sealed class HuffmanNodeComparer : IComparer<PrefixTree<T>>
        {
            public int Compare(PrefixTree<T> x, PrefixTree<T> y)
            {
                double delta = x.Probability - y.Probability;
                return delta < 0 ? -1 : delta > 0 ? 1 : 0;
            }
        }
    }
}