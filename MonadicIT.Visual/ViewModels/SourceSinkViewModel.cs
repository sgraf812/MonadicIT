using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Caliburn.Micro;
using Codeplex.Reactive;
using MonadicIT.Common;

namespace MonadicIT.Visual.ViewModels
{
    public sealed class SourceSinkViewModel : Screen, ISource
    {
        public ReactiveProperty<IEnumerable<Tuple<string, double>>> PlotData { get; private set; }

        public ReactiveProperty<IDistribution> Distribution { get; private set; }

        public SelectorViewModel<DistributionViewModel> Selector { get; private set; } 

        public SourceSinkViewModel(IEnumerable<DistributionViewModel> viewModels)
        {
            Selector = new SelectorViewModel<DistributionViewModel>(viewModels);

            Distribution = (from ai in Selector.SelectedItem
                            from dist in ai.Distribution
                            select dist).ToReactiveProperty(); // or just SelectMany
            PlotData = (from d in Distribution
                        let names = Enum.GetNames(d.SymbolType)
                        let probs = Enum.GetValues(d.SymbolType).Cast<object>().Select(o => d[o])
                        select names.Zip(probs, Tuple.Create)).ToReactiveProperty();
        }
    }
}