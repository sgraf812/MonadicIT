using System;
using System.Collections.Generic;
using MonadicIT.Common;
using Scalesque;

namespace MonadicIT.Visual.Backbone
{
    public class Transmission
    {
        public object Symbol { get; set; }
        public IEnumerable<Binary> EntropyBits { get; set; }
        public IEnumerable<Binary> ChannelBits { get; set; }
        public IEnumerable<Binary> DistortedChannelBits { get; set; }
        public IEnumerable<Binary> DistortedEntropyBits { get; set; }
        public Option<object> DistortedSymbol { get; set; }

        public override string ToString()
        {
            return string.Format("Sent: {0}, Received: {1}", Symbol,
                DistortedSymbol.HasValue ? DistortedSymbol.Get() : "<Error>");
        }
    }
}