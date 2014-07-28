using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonadicIT.Source.Lossless;

namespace MonadicIT.Hack
{
    class Program
    {
        private static readonly IDictionary<char, float> Distribution = new Dictionary<char, float>
        {
            { 'a', 0.25f }, 
            { 'b', 0.25f }, 
            { 'c', 0.5f }
        };

        private delegate IEnumerable<T> TransmissionSystem<T>(IEnumerable<T> source);

        public static void Main()
        {
            var entropy = HuffmanCoder<char>.FromProbabilities(Distribution);
            TransmissionSystem<char> system = c =>
                Sentry("Sink symbol",
                       entropy.Decode(Sentry("Entropy bit",
                                             entropy.Encode(Sentry("Source symbol",
                                                                   c)))));
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

        public static IEnumerable<char> ConsoleChars()
        {
            while (true)
            {
                foreach (var c in Console.ReadLine())
                {
                    yield return c;
                }
            }
        } 
    }
}
