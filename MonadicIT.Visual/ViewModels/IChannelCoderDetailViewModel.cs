using System;
using MonadicIT.Channel;
using MonadicIT.Common;

namespace MonadicIT.Visual.ViewModels
{
    public interface IChannelCoderDetailViewModel : ISelectable
    {
        IObservable<IChannelCoder<Binary>> ChannelCoder { get; }
    }
}