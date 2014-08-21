using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Caliburn.Micro;
using Codeplex.Reactive;
using MonadicIT.Common;
using MonadicIT.Source.Lossless;
using MonadicIT.Visual.Backbone;

namespace MonadicIT.Visual.ViewModels
{
    public class EntropyCoderViewModel : Screen, IEntropyCoderProperties
    {
        public ReactiveProperty<Func<IEnumerable<object>, IEnumerable<Binary>>> Encoder { get; private set; }

        public ReactiveProperty<Func<IEnumerable<Binary>, IEnumerable<object>>> Decoder { get; private set; }

        public ReactiveProperty<Distribution<Binary>> BitDistribution { get; private set; }

        public ReactiveProperty<IEnumerable<IPrefixTree>> CodeTree { get; private set; }

        public ReactiveProperty<IEnumerable<Tuple<object, string, double>>> CodeWords { get; private set; }

        public ReactiveProperty<double> MeanCodeWordLength { get; private set; } 

        public EntropyCoderViewModel(ISourceProperties source)
        {
            DisplayName = "Entropy coder properties";

            var coderAndDistribution = (from d in source.Distribution
                                        let m = typeof (EntropyCoderViewModel).GetMethod("HuffmanCoderFromDistribution",
                                            BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(d.SymbolType)
                                        select Tuple.Create(m.Invoke(null, new object[] {d}), d)).ToReactiveProperty();

            BitDistribution = (from cd in coderAndDistribution
                               let coder = cd.Item1
                               let dist = cd.Item2
                               let m = coder.GetType().GetMethod("GetBitDistribution", BindingFlags.Public | BindingFlags.Instance)
                               select (Distribution<Binary>)m.Invoke(coder, new object[] { dist })).ToReactiveProperty();

            Encoder = (from cd in coderAndDistribution
                       let coder = cd.Item1
                       let dist = cd.Item2
                       let m = typeof (EntropyCoderViewModel).GetMethod("HuffmanCoderEncode",
                               BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(dist.SymbolType)
                       select (Func<IEnumerable<object>, IEnumerable<Binary>>) m.Invoke(null, new[] {coder}))
                .ToReactiveProperty();

            Decoder = (from cd in coderAndDistribution
                       let coder = cd.Item1
                       let dist = cd.Item2
                       let m = typeof (EntropyCoderViewModel).GetMethod("HuffmanCoderDecode",
                               BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(dist.SymbolType)
                       select (Func<IEnumerable<Binary>, IEnumerable<object>>) m.Invoke(null, new[] {coder}))
                .ToReactiveProperty();

            CodeTree = (from cd in coderAndDistribution
                        let coder = cd.Item1
                        let p = coder.GetType().GetProperty("CodeTree", BindingFlags.Public | BindingFlags.Instance)
                        select ((IPrefixTree) p.GetValue(coder)).SingleValue()).ToReactiveProperty();

            CodeWords = (from cd in coderAndDistribution
                         let coder = cd.Item1
                         let dist = cd.Item2
                         let m = typeof (EntropyCoderViewModel).GetMethod("HuffmanCoderGetCodeWords",
                             BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(dist.SymbolType)
                         select (IEnumerable<Tuple<object, string, double>>) m.Invoke(null, new[] {coder, dist}))
                .ToReactiveProperty();

            MeanCodeWordLength = (from codeWords in CodeWords
                                  select codeWords.Sum(cw => cw.Item2.Length*cw.Item3)).ToReactiveProperty();
        }

// ReSharper disable UnusedMember.Local
        private static HuffmanCoder<T> HuffmanCoderFromDistribution<T>(IDistribution distribution) where T : struct
        {
            return HuffmanCoder<T>.FromDistribution((Distribution<T>) distribution);
        }

        private static Func<IEnumerable<object>, IEnumerable<Binary>> HuffmanCoderEncode<T>(object coder) where T : struct
        {
            var huffmanCoder = (HuffmanCoder<T>)coder;
            return s => huffmanCoder.Encode(s.Cast<T>());
        }

        private static Func<IEnumerable<Binary>, IEnumerable<object>> HuffmanCoderDecode<T>(object coder) where T : struct
        {
            var huffmanCoder = (HuffmanCoder<T>)coder;
            return s => huffmanCoder.Decode(s).Cast<object>();
        }

        private static IEnumerable<Tuple<object, string, double>> HuffmanCoderGetCodeWords<T>(object coder, IDistribution distribution) where T : struct
        {
            var huffmanCoder = (HuffmanCoder<T>) coder;
            return from p in huffmanCoder.CodeDictionary
                   let symbol = (object)p.Key
                   let codeWord = string.Join("", p.Value.Select(c => c.ToString()))
                   let prob = distribution[symbol]
                   orderby prob descending
                   select Tuple.Create(symbol, codeWord, prob);
        }
// ReSharper restore UnusedMember.Local

        IObservable<Func<IEnumerable<object>, IEnumerable<Binary>>> IEntropyCoderProperties.Encoder
        {
            get { return Encoder; }
        }

        IObservable<Func<IEnumerable<Binary>, IEnumerable<object>>> IEntropyCoderProperties.Decoder
        {
            get { return Decoder; }
        }
    }
}