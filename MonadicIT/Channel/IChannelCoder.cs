using System.Collections;
using System.Collections.Generic;
using MonadicIT.Common;

namespace MonadicIT.Channel
{
    public interface IChannelCoder<T>
    {
        IEnumerable<T> Encode(IEnumerable<Binary> bits);

        IEnumerable<Binary> Decode(IEnumerable<T> code);
    }
}