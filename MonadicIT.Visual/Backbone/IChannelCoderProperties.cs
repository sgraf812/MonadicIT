using System;
using MonadicIT.Channel;
using MonadicIT.Common;

namespace MonadicIT.Visual.Backbone
{
    public interface IChannelCoderProperties
    {
        IObservable<IChannelCoder<Binary>> Coder { get; }
    }
}