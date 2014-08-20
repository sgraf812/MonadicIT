using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Codeplex.Reactive;
using MonadicIT.Channel;
using MonadicIT.Common;

namespace MonadicIT.Visual.ViewModels
{
    public class ChannelCoderViewModel : Screen
    {
        public IObservable<IChannelCoder<Binary>> ChannelCoder { get; private set; }

        public SelectorViewModel<IChannelCoderDetailViewModel> Selector { get; private set; } 

        public ChannelCoderViewModel(IEnumerable<IChannelCoderDetailViewModel> channelCoderDetailViewModels)
        {
            Selector = new SelectorViewModel<IChannelCoderDetailViewModel>(channelCoderDetailViewModels);
        }
    }

    public interface IChannelCoderDetailViewModel : ISelectable
    {
    }
}