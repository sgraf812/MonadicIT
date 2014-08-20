using System;
using System.Collections.Generic;
using System.Linq;
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

        public ReactiveProperty<IEnumerable<Tuple<string, double>>> PlotData { get; private set; }

        public ChannelCoderViewModel(
            IEnumerable<IChannelCoderDetailViewModel> channelCoderDetailViewModels, 
            IChannelSettings channel)
        {
            DisplayName = "Channel coder properties";

            Selector = new SelectorViewModel<IChannelCoderDetailViewModel>(channelCoderDetailViewModels);

            Coder = from detail in Selector.SelectedItem
                    from coder in detail.ChannelCoder
                    select coder;

            PlotData = (from coder in Coder
                        from ch in channel.Channel
                        select
                            new[]
                            {
                                Tuple.Create("Code rate", coder.CodeRate),
                                Tuple.Create("Channel capacity", ch.ChannelCapacity),
                                Tuple.Create("Residual error rate", coder.ResidualErrorRate(ch)),
                                Tuple.Create("Channel error rate", ch.ErrorRate()),
                            }.AsEnumerable()).ToReactiveProperty();
        }
    }
}