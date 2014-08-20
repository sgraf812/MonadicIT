using Codeplex.Reactive;
using MonadicIT.Common;

namespace MonadicIT.Visual.Backbone
{
    public interface ISourceSettings
    {
        ReactiveProperty<IDistribution> Distribution { get; } 
    }
}