using System;
using System.Reactive.Linq;
using Codeplex.Reactive;
using MonadicIT.Channel;
using MonadicIT.Common;

namespace MonadicIT.Visual.ViewModels
{
    public class HammingCodeViewModel : SelectableViewModelBase, IChannelCoderDetailViewModel
    {
        public HammingCodeViewModel()
        {
            ParityBits = new ReactiveProperty<int>(2);
            ChannelCoder = ParityBits.Select(m => new HammingCode(m));
            BlockLength = ChannelCoder.Select(c => c.BlockLength).ToReactiveProperty();
        }

        public ReactiveProperty<int> ParityBits { get; private set; }
        public ReactiveProperty<int> BlockLength { get; private set; }

        public override string DisplayName
        {
            get { return "Hamming code"; }
        }

        public IObservable<IChannelCoder<Binary>> ChannelCoder { get; private set; }
    }
}