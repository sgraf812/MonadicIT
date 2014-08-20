using System.Collections.Generic;
using MonadicIT.Common;

namespace MonadicIT.Channel
{
    public interface IChannelCoder<T> where T : /* Enum, */ struct
    {
        double CodeRate { get; }

        IEnumerable<T> Encode(IEnumerable<Binary> bits);

        IEnumerable<Binary> Decode(IEnumerable<T> code);

        double ResidualErrorRate(IDiscreteChannel<T> channel);
    }
}