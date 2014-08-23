using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Caliburn.Micro;
using Codeplex.Reactive;
using MonadicIT.Common;
using MonadicIT.Visual.Backbone;

namespace MonadicIT.Visual.ViewModels
{
    public sealed class SourceSinkViewModel : Screen, ISourceProperties
    {
        public SourceSinkViewModel(IEnumerable<DistributionViewModel> viewModels)
        {
            DisplayName = "Source distribution properties";

            Selector = new SelectorViewModel<DistributionViewModel>(viewModels);

            SymbolRate = new ReactiveProperty<int>(0);

            Distribution = (from ai in Selector.SelectedItem
                            from dist in ai.Distribution
                            select dist).ToReactiveProperty(); // or just SelectMany

            PlotData = (from d in Distribution
                        let names = Enum.GetNames(d.SymbolType)
                        let probs = Enum.GetValues(d.SymbolType).Cast<object>().Select(o => d[o])
                        select names.Zip(probs, Tuple.Create)).ToReactiveProperty();
        }

        public ReactiveProperty<IEnumerable<Tuple<string, double>>> PlotData { get; private set; }

        public ReactiveProperty<IDistribution> Distribution { get; private set; }

        public ReactiveProperty<int> SymbolRate { get; private set; }

        public SelectorViewModel<DistributionViewModel> Selector { get; private set; }

        IObservable<IDistribution> ISourceProperties.Distribution
        {
            get { return Distribution; }
        }

        IObservable<int> ISourceProperties.SymbolRate
        {
            get { return SymbolRate; }
        }
    }
}