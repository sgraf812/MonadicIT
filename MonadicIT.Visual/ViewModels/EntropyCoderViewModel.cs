using System;
using System.Collections.Generic;
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
        private readonly ISource _source;
        public ReactiveProperty<Func<IEnumerable<object>, IEnumerable<Binary>>> Encoder { get; private set; }
        public ReactiveProperty<Func<IEnumerable<Binary>, IEnumerable<object>>> Decoder { get; private set; }
        public ReactiveProperty<Distribution<Binary>> BitDistribution { get; private set; } 

        public EntropyCoderViewModel(ISource source)
        {
            _source = source;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            var coderAndDistribution = from d in _source.Distribution
                                       let t = typeof (EntropyCoderViewModel).MakeGenericType(d.SymbolType)
                                       let m =
                                           t.GetMethod("HuffmanCoderFromDistribution",
                                               BindingFlags.NonPublic | BindingFlags.Static)
                                       select Tuple.Create(m.Invoke(null, new object[] {d}), d);

            BitDistribution = (from cd in coderAndDistribution
                               let coder = cd.Item1
                               let dist = cd.Item2
                               let m = coder.GetType().GetMethod("GetBitDistribution", BindingFlags.Public)
                               select (Distribution<Binary>) m.Invoke(coder, new[] {dist})).ToReactiveProperty();

            Encoder = (from cd in coderAndDistribution
                       let coder = cd.Item1
                       let dist = cd.Item2
                       let symbolType = dist.SymbolType
                       let m = typeof (EntropyCoderViewModel).GetMethod("HuffmanCoderEncode")
                           .MakeGenericMethod(symbolType)
                       select (Func<IEnumerable<object>, IEnumerable<Binary>>) m.Invoke(null, new[] {coder}))
                .ToReactiveProperty();

            Decoder = (from cd in coderAndDistribution
                       let coder = cd.Item1
                       let dist = cd.Item2
                       let symbolType = dist.SymbolType
                       let m = typeof (EntropyCoderViewModel).GetMethod("HuffmanCoderDecode")
                           .MakeGenericMethod(symbolType)
                       select (Func<IEnumerable<Binary>, IEnumerable<object>>) m.Invoke(null, new[] {coder}))
                .ToReactiveProperty();
        }

        private static HuffmanCoder<T> HuffmanCoderFromDistribution<T>(IDistribution distribution) where T : struct
        {
            return HuffmanCoder<T>.FromDistribution((Distribution<T>) distribution);
        }

        private static Distribution<Binary> HuffmanCoderGetBitDistribution<T>(object coder, IDistribution distribution) where T : struct
        {
            var huffmanCoder = (HuffmanCoder<T>) coder;
            return huffmanCoder.GetBitDistribution((Distribution<T>) distribution);
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
    }
}