using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Caliburn.Micro;
using Codeplex.Reactive;
using MonadicIT.Common;
using MonadicIT.Source.Lossless;

namespace MonadicIT.Visual.ViewModels
{
    public class EntropyCoderViewModel : Screen
    {
        private readonly IDisposable _subscription;
        public ReactiveProperty<Func<IEnumerable<object>, IEnumerable<Binary>>> Encoder { get; private set; }
        public ReactiveProperty<Func<IEnumerable<Binary>, IEnumerable<object>>> Decoder { get; private set; }
        public ReactiveProperty<Distribution<Binary>> BitDistribution { get; private set; } 

        public EntropyCoderViewModel(ISource source)
        {
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
        }

        private static void Hi(Distribution<Binary> distribution)
        {
            
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
// ReSharper restore UnusedMember.Local
    }
}