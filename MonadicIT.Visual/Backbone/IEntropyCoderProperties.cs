using System;
using System.Collections.Generic;
using MonadicIT.Common;

namespace MonadicIT.Visual.Backbone
{
    public interface IEntropyCoderProperties
    {
        IObservable<Func<IEnumerable<object>, IEnumerable<Binary>>> Encoder { get; }

        IObservable<Func<IEnumerable<Binary>, IEnumerable<object>>> Decoder { get; }
    }
}