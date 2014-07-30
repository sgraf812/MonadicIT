using System.Collections.Generic;
using MonadicIT.Common;

namespace MonadicIT.Channel
{
    public class BlockCoder<T> : IChannelCoder<T>
    {
        public IEnumerable<T> Encode(IEnumerable<Binary> bits)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Binary> Decode(IEnumerable<T> code)
        {
            throw new System.NotImplementedException();
        }
    }
}