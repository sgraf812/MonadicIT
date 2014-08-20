using System;
using System.Collections.Generic;
using System.Linq;
using MonadicIT.Channel;
using MonadicIT.Common;
using MonadicIT.Source.Lossless;

namespace MonadicIT.Hack
{
    class Program
    {
        private static readonly Distribution<Ternary> Distribution = Distribution<Ternary>.FromProbabilites(new[]
        {
            Tuple.Create(Ternary.Zero, 0.25),
            Tuple.Create(Ternary.One, 0.25),
            Tuple.Create(Ternary.Two, 0.5),
        });

        private delegate IEnumerable<T> TransmissionSystem<T>(IEnumerable<T> source);

        public static void Main()
        {
            var channel = new SymmetricChannel<Binary>(0.99);
            var mm = Enumerable.Range(2, 20).Max(m =>
            {
                var c = new HammingCode(m);
                return Tuple.Create(c.CodeRate*(1 - c.ResidualErrorRate(channel)), m);
            });
            var channelCode = new HammingCode(mm.Item2);
            var huffman = HuffmanCoder<Ternary>.FromDistribution(Distribution);

            Console.WriteLine("Source Entropy: {0}", Distribution.Entropy);
            Console.WriteLine("Channel Capacity: {0}", channel.ChannelCapacity);
            Console.WriteLine("Channel code rate: {0}", channelCode.CodeRate);


            TransmissionSystem<Ternary> system = source =>
            {
                var encEntropyBits = huffman.Encode(Sentry("Source", source));
                var encChannelBits = channelCode.Encode(Sentry("Enc entropy bit", encEntropyBits));
                var decChannelBits = Distort(channel)(Sentry("Enc channel bit", encChannelBits));
                var decEntropyBits = channelCode.Decode(Sentry("Dec channel bit", decChannelBits));
                var sink = huffman.Decode(Sentry("Dec entropy bit", decEntropyBits));
                return Sentry("Sink", sink);
            };
            TransmissionSystem<Ternary> system2 = source =>
            {
                var encEntropyBits = huffman.Encode(Sentry("Source", source));
                var decEntropyBits = Distort(channel)(Sentry("Enc channel bit", encEntropyBits));
                var sink = huffman.Decode(Sentry("Dec entropy bit", decEntropyBits));
                return Sentry("Sink", sink);
            };

            TransmissionSystem<Binary> channelSystem = bits =>
            {
                var encChannelBits = channelCode.Encode(Sentry("Enc entropy bit", bits));
                var decChannelBits = Distort(channel)(Sentry("Enc channel bit", encChannelBits));
                var decEntropyBits = channelCode.Decode(Sentry("Dec channel bit", decChannelBits));
                return decEntropyBits;
            };
            var channelSystem2 = new TransmissionSystem<Binary>(Distort(channel));

            Console.WriteLine("Estimated correct transmission rates for one bit: without code: {0} with code: {1}", 1-channel.ErrorRate(), 1-channelCode.ResidualErrorRate(channel));
            var nbits = channelCode.ParityBits;
            double without10 = MathHelper.KOutOfNProbability(nbits, 0, channel.ErrorRate());
            double with10 = 1-channelCode.ResidualErrorRate(channel);
            Console.WriteLine("Estimated correct transmission rates for blocks of {2} bits: without code: {0} with code: {1}", without10, with10, nbits);

            var n = 0;
            var p1 = 0;
            var p2 = 0;
            var bitDist = Distribution<Binary>.Uniform(EnumHelper<Binary>.Values);
            while (true)
            {
                var seq = Enumerable.Range(0, nbits).Select(_ => bitDist.Sample()).ToArray();
                n++;
                if (CorrectTransmission(channelSystem, seq))
                {
                    p1++;
                }
                if (CorrectTransmission(channelSystem2, seq))
                {
                    p2++;
                }
                if (n% (100000/nbits) == 0)
                Console.WriteLine("without channel code: {0}, with: {1}", p2/(double) n, p1/(double) n);
            }
        }

        private static bool CorrectTransmission<T>(TransmissionSystem<T> system, ICollection<T> seq)
        {
            try
            {
                return system(seq).Zip(seq, (a, b) => a.Equals(b)).PadWith(false, seq.Count).Take(seq.Count).All(b => b);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static Func<IEnumerable<T>, IEnumerable<T>>  Distort<T>(IDiscreteChannel<T> channel) where T : /* Enum, */ struct
        {
            return e => from s in e
                        select channel.GetTransitionDistribution(s).Sample();
        }

        public static IEnumerable<T> Sentry<T>(string prefix, IEnumerable<T> ts)
        {
            foreach (var t in ts)
            {
                //Console.WriteLine("{0}: {1}", prefix, t);
                yield return t;
            }
        }

        public static IEnumerable<Ternary> ConsoleChars()
        {
            while (true)
            {
                foreach (var c in Console.ReadLine())
                {
                    switch (c)
                    {
                        case '0':
                            yield return Ternary.Zero;
                            break;
                        case '1':
                            yield return Ternary.One;
                            break;
                        case '2':
                            yield return Ternary.Two;
                            break;
                    }
                }
            }
        } 
    }
}
