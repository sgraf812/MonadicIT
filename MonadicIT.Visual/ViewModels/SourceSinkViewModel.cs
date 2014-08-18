using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using Caliburn.Micro;
using Codeplex.Reactive;
using Codeplex.Reactive.Extensions;
using MonadicIT.Common;

namespace MonadicIT.Visual.ViewModels
{
    public sealed class SourceSinkViewModel : Conductor<DistributionViewModel>.Collection.OneActive, ISource
    {
        public ReactiveProperty<IEnumerable<Tuple<string, double>>> PlotData { get; private set; }

        public ReactiveProperty<IDistribution> Distribution { get; private set; } 

        public SourceSinkViewModel(IEnumerable<DistributionViewModel> distributions)
        {
            Items.AddRange(distributions);
            ActivateItem(Items[0]);

            var activeItem = this.ObserveProperty(x => x.ActiveItem);
            Distribution = (from ai in activeItem
                            from dist in ai.Distribution
                            select dist).ToReactiveProperty(); // or just SelectMany
            PlotData = (from d in Distribution
                        let names = Enum.GetNames(d.SymbolType)
                        let probs = Enum.GetValues(d.SymbolType).Cast<object>().Select(o => d[o])
                        select names.Zip(probs, Tuple.Create)).ToReactiveProperty();
        }
    }
}