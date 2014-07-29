using System.Collections.Generic;
using MonadicIT.Common;

namespace MonadicIT.Source
{
    public interface ISourceDecoder<out T> where T : /* Enum, */ struct
    {
        IEnumerable<T> Decode(IEnumerable<Binary> bits);
    }
}