using System;
using MonadicIT.Channel;
using MonadicIT.Common;

namespace MonadicIT.Visual.Backbone
{
    public interface IChannelProperties
    {
        IObservable<IDiscreteChannel<Binary>> Channel { get; }
    }
}