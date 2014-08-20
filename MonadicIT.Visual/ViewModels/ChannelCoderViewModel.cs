using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Caliburn.Micro;
using Codeplex.Reactive;
using MonadicIT.Channel;
using MonadicIT.Common;
using MonadicIT.Visual.Backbone;

namespace MonadicIT.Visual.ViewModels
{
    public class ChannelCoderViewModel : Screen
    {
        public IObservable<IChannelCoder<Binary>> Coder { get; private set; }

        public SelectorViewModel<IChannelCoderDetailViewModel> Selector { get; private set; }

        public ReactiveProperty<double> CodeRate { get; private set; }

        public ReactiveProperty<int> BlockLength { get; private set; } 

        public ReactiveProperty<double> ResidualErrorRate { get; private set; }  

        public ChannelCoderViewModel(
            IEnumerable<IChannelCoderDetailViewModel> channelCoderDetailViewModels, 
            IChannelSettings channel)
        {
            Selector = new SelectorViewModel<IChannelCoderDetailViewModel>(channelCoderDetailViewModels);

            Coder = from detail in Selector.SelectedItem
                    from coder in detail.ChannelCoder
                    select coder;

            CodeRate = Coder.Select(c => c.CodeRate).ToReactiveProperty();
            BlockLength = Coder.Select(c => c.BlockLength).ToReactiveProperty();
            ResidualErrorRate = (from code in Coder
                                 from ch in channel.Channel
                                 select code.ResidualErrorRate(ch)).ToReactiveProperty();
        }
    }
}