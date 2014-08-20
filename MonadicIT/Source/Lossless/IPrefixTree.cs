using System.Collections.Generic;

namespace MonadicIT.Source.Lossless
{
    public interface IPrefixTree
    {
        double Probability { get; set; }
        bool IsLeaf { get; }
        object Value { get; }
        IPrefixTree Left { get; }
        IPrefixTree Right { get; }
        IEnumerable<IPrefixTree> Children { get; }
    }
}