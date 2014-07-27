using System.Collections;
using System.Collections.Generic;

namespace MonadicIT.Source
{
    public interface ISourceDecoder<out T>
    {
        IEnumerable<T> Decode(IEnumerable<bool> bits);
    }
}