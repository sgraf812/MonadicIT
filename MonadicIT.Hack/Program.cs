using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonadicIT.Channel;
using MonadicIT.Common;
using MonadicIT.Source;
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
            Console.WriteLine("Source Entropy: {0}", Distribution.Entropy);

            var channel = new SymmetricChannel<Binary>(0.95);
            var huffman = HuffmanCoder<Ternary>.FromDistribution(Distribution);
            TransmissionSystem<Ternary> system = source =>
            {
                var bits = huffman.Encode(Sentry("Source", source));
                var sink = huffman.Decode(Sentry("Entropy bit", bits));
                return Sentry("Sink", sink);
            };

            foreach (var c in system(ConsoleChars()))
            {
                Console.WriteLine();
            }
        }

        public static IEnumerable<T> Sentry<T>(string prefix, IEnumerable<T> ts)
        {
            foreach (var t in ts)
            {
                Console.WriteLine("{0}: {1}", prefix, t);
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
