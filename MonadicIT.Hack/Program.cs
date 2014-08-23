using System;
using System.Collections.Generic;
using System.Linq;
using MonadicIT.Channel;
using MonadicIT.Common;
using MonadicIT.Source.Lossless;

namespace MonadicIT.Hack
{
    internal class Program
    {
        private static readonly Distribution<Ternary> Distribution = Distribution<Ternary>.FromProbabilites(new[]
        {
            Tuple.Create(Ternary.Zero, 0.25),
            Tuple.Create(Ternary.One, 0.25),
            Tuple.Create(Ternary.Two, 0.5)
        });

        public static void Main()
        {
            var channel = new SymmetricChannel<Binary>(0.99);
            Tuple<double, int> mm = Enumerable.Range(2, 20).Max(m =>
            {
                var c = new HammingCode(m);
                return Tuple.Create(c.CodeRate*(1 - c.ResidualErrorRate(channel)), m);
            });
            var channelCode = new HammingCode(mm.Item2);
            HuffmanCoder<Ternary> huffman = HuffmanCoder<Ternary>.FromDistribution(Distribution);

            Console.WriteLine("Source Entropy: {0}", Distribution.Entropy);
            Console.WriteLine("Channel Capacity: {0}", channel.ChannelCapacity);
            Console.WriteLine("Channel code rate: {0}", channelCode.CodeRate);


            TransmissionSystem<Ternary> system = source =>
            {
                IEnumerable<Binary> encEntropyBits = huffman.Encode(Sentry("Source", source));
                IEnumerable<Binary> encChannelBits = channelCode.Encode(Sentry("Enc entropy bit", encEntropyBits));
                IEnumerable<Binary> decChannelBits = Distort(channel)(Sentry("Enc channel bit", encChannelBits));
                IEnumerable<Binary> decEntropyBits = channelCode.Decode(Sentry("Dec channel bit", decChannelBits));
                IEnumerable<Ternary> sink = huffman.Decode(Sentry("Dec entropy bit", decEntropyBits));
                return Sentry("Sink", sink);
            };
            TransmissionSystem<Ternary> system2 = source =>
            {
                IEnumerable<Binary> encEntropyBits = huffman.Encode(Sentry("Source", source));
                IEnumerable<Binary> decEntropyBits = Distort(channel)(Sentry("Enc channel bit", encEntropyBits));
                IEnumerable<Ternary> sink = huffman.Decode(Sentry("Dec entropy bit", decEntropyBits));
                return Sentry("Sink", sink);
            };

            TransmissionSystem<Binary> channelSystem = bits =>
            {
                IEnumerable<Binary> encChannelBits = channelCode.Encode(Sentry("Enc entropy bit", bits));
                IEnumerable<Binary> decChannelBits = Distort(channel)(Sentry("Enc channel bit", encChannelBits));
                IEnumerable<Binary> decEntropyBits = channelCode.Decode(Sentry("Dec channel bit", decChannelBits));
                return decEntropyBits;
            };
            var channelSystem2 = new TransmissionSystem<Binary>(Distort(channel));

            Console.WriteLine("Estimated correct transmission rates for one bit: without code: {0} with code: {1}",
                1 - channel.ErrorRate(), 1 - channelCode.ResidualErrorRate(channel));
            int nbits = channelCode.ParityBits;
            double without10 = MathHelper.KOutOfNProbability(nbits, 0, channel.ErrorRate());
            double with10 = 1 - channelCode.ResidualErrorRate(channel);
            Console.WriteLine(
                "Estimated correct transmission rates for blocks of {2} bits: without code: {0} with code: {1}",
                without10, with10, nbits);

            int n = 0;
            int p1 = 0;
            int p2 = 0;
            Distribution<Binary> bitDist = Distribution<Binary>.Uniform(EnumHelper<Binary>.Values);
            while (true)
            {
                Binary[] seq = Enumerable.Range(0, nbits).Select(_ => bitDist.Sample()).ToArray();
                n++;
                if (CorrectTransmission(channelSystem, seq))
                {
                    p1++;
                }
                if (CorrectTransmission(channelSystem2, seq))
                {
                    p2++;
                }
                if (n%(100000/nbits) == 0)
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

        private static Func<IEnumerable<T>, IEnumerable<T>> Distort<T>(IDiscreteChannel<T> channel)
            where T : /* Enum, */ struct
        {
            return e => from s in e
                        select channel.GetTransitionDistribution(s).Sample();
        }

        public static IEnumerable<T> Sentry<T>(string prefix, IEnumerable<T> ts)
        {
            foreach (T t in ts)
            {
                //Console.WriteLine("{0}: {1}", prefix, t);
                yield return t;
            }
        }

        public static IEnumerable<Ternary> ConsoleChars()
        {
            while (true)
            {
                foreach (char c in Console.ReadLine())
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

        private delegate IEnumerable<T> TransmissionSystem<T>(IEnumerable<T> source);
    }
}