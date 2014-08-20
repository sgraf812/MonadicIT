using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Caliburn.Micro;
using Codeplex.Reactive;
using Codeplex.Reactive.Extensions;
using MonadicIT.Common;

namespace MonadicIT.Visual.ViewModels
{
    public sealed class SourceSinkViewModel : Screen, ISource
    {
        private readonly IList<DistributionViewModel> _distributionViewModels;

        public IList<DistributionViewModel> Items { get { return _distributionViewModels; } }
        public ReactiveProperty<DistributionViewModel> ActiveItem { get; private set; }

        public ReactiveProperty<IEnumerable<Tuple<string, double>>> PlotData { get; private set; }

        public ReactiveProperty<IDistribution> Distribution { get; private set; }

        public SourceSinkViewModel(IEnumerable<DistributionViewModel> distributionViewModels)
        {
            _distributionViewModels = distributionViewModels.ToList();

            var activeItemStreams = from dvm in _distributionViewModels
                                    let activeItem = from b in dvm.ObserveProperty(x => x.IsActive)
                                                     where b
                                                     select dvm
                                    select activeItem;
            ActiveItem = activeItemStreams.Merge().ToReactiveProperty();

            _distributionViewModels.First().IsActive = true;

            Distribution = (from ai in ActiveItem
                            from dist in ai.Distribution
                            select dist).ToReactiveProperty(); // or just SelectMany
            PlotData = (from d in Distribution
                        let names = Enum.GetNames(d.SymbolType)
                        let probs = Enum.GetValues(d.SymbolType).Cast<object>().Select(o => d[o])
                        select names.Zip(probs, Tuple.Create)).ToReactiveProperty();
        }
    }
}