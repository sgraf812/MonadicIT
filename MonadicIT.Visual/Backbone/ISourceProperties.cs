using System;
using MonadicIT.Common;

namespace MonadicIT.Visual.Backbone
{
    public interface ISourceProperties
    {
        IObservable<IDistribution> Distribution { get; }
        IObservable<int> SymbolRate { get; }
    }
}