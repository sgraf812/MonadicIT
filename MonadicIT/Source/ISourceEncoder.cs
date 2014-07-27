using System.Collections.Generic;

namespace MonadicIT.Source
{
    public interface ISourceEncoder<in T>
    {
        IEnumerable<bool> Encode(IEnumerable<T> symbols);
    }
}