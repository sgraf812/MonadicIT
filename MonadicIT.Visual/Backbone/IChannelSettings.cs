using System;
using MonadicIT.Channel;
using MonadicIT.Common;

namespace MonadicIT.Visual.Backbone
{
    public interface IChannelSettings
    {
        IObservable<IDiscreteChannel<Binary>> Channel { get; }
    }
}