using System.Collections.Generic;
using MonadicIT.Common;

namespace MonadicIT.Channel
{
    public class IdentityCoder : IChannelCoder<Binary>
    {
        public double CodeRate { get { return 1; } }

        public IEnumerable<Binary> Encode(IEnumerable<Binary> bits)
        {
            return bits;
        }

        public IEnumerable<Binary> Decode(IEnumerable<Binary> code)
        {
            return code;
        }

        public double ResidualErrorRate(IDiscreteChannel<Binary> channel)
        {
            return channel.ErrorRate();
        }
    }
}