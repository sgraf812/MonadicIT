using System.Collections.Generic;
using MonadicIT.Common;

namespace MonadicIT.Source
{
    public interface ISourceEncoder<in T> where T : /* Enum, */ struct
    {
        IEnumerable<Binary> Encode(IEnumerable<T> symbols);
    }
}