using System;
using System.Reactive.Linq;
using Codeplex.Reactive;
using MonadicIT.Channel;
using MonadicIT.Common;

namespace MonadicIT.Visual.ViewModels
{
    public class HammingCodeViewModel : SelectableViewModelBase, IChannelCoderDetailViewModel
    {
        public override string DisplayName { get { return "Hamming code"; } }
        public IObservable<IChannelCoder<Binary>> ChannelCoder { get; private set; }
        public ReactiveProperty<int> ParityBits { get; private set; } 

        public HammingCodeViewModel()
        {
            ParityBits = new ReactiveProperty<int>(2);
            ChannelCoder = from m in ParityBits select new HammingCode(m);
        }
    }
}