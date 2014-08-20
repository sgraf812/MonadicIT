using System;
using System.Reactive.Linq;
using Caliburn.Micro;
using Codeplex.Reactive;
using MonadicIT.Channel;
using MonadicIT.Common;
using MonadicIT.Visual.Backbone;

namespace MonadicIT.Visual.ViewModels
{
    public class ChannelViewModel : PropertyChangedBase, IChannelSettings
    {
        public IObservable<IDiscreteChannel<Binary>> Channel { get; private set; }

        public ReactiveProperty<double> ErrorProbability { get; private set; }

        public ReactiveProperty<double> ChannelCapacity { get; private set; }

        public ChannelViewModel()
        {
            ErrorProbability = new ReactiveProperty<double>(0);

            Channel = from pe in ErrorProbability
                      select new SymmetricChannel<Binary>(1 - pe);

            ChannelCapacity = Channel.Select(x => x.ChannelCapacity).ToReactiveProperty();
        }
    }
}