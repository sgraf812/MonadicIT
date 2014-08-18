using Codeplex.Reactive;
using MonadicIT.Common;

namespace MonadicIT.Visual.ViewModels
{
    public interface ISource
    {
        ReactiveProperty<IDistribution> Distribution { get; } 
    }
}