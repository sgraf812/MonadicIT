﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using MonadicIT.Common;

namespace MonadicIT.Channel
{
    public class HammingCode : IChannelCoder<Binary>
    {
        public int N { get; private set; }
        public int K { get; private set; }

        public HammingCode(int m)
        {
            if (m < 2) throw new ArgumentOutOfRangeException("m", "Must be at least 2");
            N = MaxNForParityBits(m);
            K = N - m;
        }

        public static int MaxNForParityBits(int m)
        {
            return (1 << m) - 1; // 2^m - 1
        }

        public IEnumerable<Binary> Encode(IEnumerable<Binary> bits)
        {
            // reused buffer
            var codeBlock = new Binary[N];

            foreach (var block in bits.InChunksOf(K))
            {
                // fill up the last chunk with 0s
                var paddedBlock = block.Count < K ? block.Concat(Binary.O.Repeat()).Take(K).ToArray() : block;

                var m = 0; // running number of parity bits
                var n = 0; // running number of all bits
                foreach (var bit in paddedBlock)
                {
                    while (n >= MaxNForParityBits(m))
                    {
                        // pad with a parity bit until we can insert another information bit
                        // we will calculate the actual parity in a later pass
                        // Since we'll have even parity, initialize with 0s.
                        codeBlock[n++] = Binary.O;
                        m++;
                    }

                    // postcondition: codeBlock.Count < N, so we can safely insert an information bit
                    codeBlock[n++] = bit;
                }

                // first we compute all parity bits of the block
                for (var i = 0; i < m; i++)
                {
                    // i = 0 => idx = 0, len = 1: xor bit 0, 2, 4, 6, ... 
                    // i = 2 => idx = 3, len = 4: xor bit 3,4,5,6, 11,12,13,14 ...
                    // find the parity index (idx) and xor stride (stride)
                    var idx = (1 << i) - 1;
                    var stride = (1 << i);
                    codeBlock[idx] = ComputeParityBit(codeBlock, idx, stride);
                }

                Debug.Assert(n == N, "Didn't insert the right number of parity bits.");
                foreach (var bit in codeBlock)
                {
                    yield return bit;
                }
            }
        }

        private static Binary ComputeParityBit(IEnumerable<Binary> codeBlock, int idx, int stride)
        {
            return codeBlock
                .Skip(idx)
                .AlternatingTakeAndSkip(stride)
                .Aggregate(Binary.O, Xor);
        }

        private static Binary Xor(Binary a, Binary b)
        {
            return a != b ? Binary.I : Binary.O;
        }

        private static Binary Not(Binary b)
        {
            return Xor(b, Binary.I);
        }

        public IEnumerable<Binary> Decode(IEnumerable<Binary> code)
        {
            foreach (var codeBlock in code.InChunksOf(N))
            {
                var M = N - K;
                var bitToFix = 0; // one-based index into the array, so 0 actually means that there was no error
                for (var i = 0; i < M; i++)
                {
                    // compute parities the same way as in the encoder
                    // parity bits should all be zero if no transmission error was detected.
                    var idx = (1 << i) - 1;
                    var stride = (1 << i);
                    if (ComputeParityBit(codeBlock, idx, stride) != Binary.O)
                    {
                        // There was a transmission error detected. Add stride to our fix index
                        // This utilizes the special distribution of parity bits.
                        bitToFix += stride;
                    }
                }

                var n = 0;
                var m = 0;
                foreach (var bit in codeBlock)
                {
                    if (n >= MaxNForParityBits(m))
                    {
                        m++; // don't emit parity bits
                    }
                    else
                    {
                        if (n == bitToFix - 1)
                        {
                            // There was a transmission error detected and we can correct the offending bit, 
                            // which is at index bitToFix-1.
                            yield return Not(bit);
                        }
                        else
                        {
                            yield return bit;
                        }
                    }
                    n++;
                }

                Debug.Assert(m == N - K, "Didn't extract the right number of parity bits.");
            }
        }
    }
}