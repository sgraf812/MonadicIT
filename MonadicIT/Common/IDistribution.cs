using System;

namespace MonadicIT.Common
{
    public interface IDistribution
    {
        Type SymbolType { get; }
        double this[object symbol] { get; }
        double Entropy { get; }
    }
}