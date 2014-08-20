using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Caliburn.Micro;
using MonadicIT.Channel;
using MonadicIT.Common;

namespace MonadicIT.Visual.ViewModels
{
    public class ChannelCoderViewModel : Screen
    {
        public IObservable<IChannelCoder<Binary>> Coder { get; private set; }

        public SelectorViewModel<IChannelCoderDetailViewModel> Selector { get; private set; } 

        public ChannelCoderViewModel(IEnumerable<IChannelCoderDetailViewModel> channelCoderDetailViewModels)
        {
            Selector = new SelectorViewModel<IChannelCoderDetailViewModel>(channelCoderDetailViewModels);

            Coder = from detail in Selector.SelectedItem
                    from coder in detail.ChannelCoder
                    select coder;
        }
    }
}