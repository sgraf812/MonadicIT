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
        public ReactiveProperty<int> M { get; private set; } 

        public HammingCodeViewModel()
        {
            M = new ReactiveProperty<int>(2);
            ChannelCoder = from m in M select new HammingCode(m);
        }
    }
}