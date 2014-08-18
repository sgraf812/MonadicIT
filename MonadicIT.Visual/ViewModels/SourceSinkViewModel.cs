using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Windows;
using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using Codeplex.Reactive;
using Codeplex.Reactive.Extensions;
using MonadicIT.Common;
using OxyPlot;
using OxyPlot.Series;
using ReactiveUI;

namespace MonadicIT.Visual.ViewModels
{
    public class SourceSinkViewModel : Conductor<DistributionViewModel>.Collection.OneActive
    {
        private readonly PlotModel model = new PlotModel();
        private readonly ColumnSeries columns = new ColumnSeries();

        public ReactiveProperty<PlotModel> Histogram { get; private set; }

        public IObservable<IDistribution> Distribution { get; private set; } 

        public SourceSinkViewModel(IEnumerable<DistributionViewModel> distributions)
        {
            Items.AddRange(distributions);
            var activeItem = this.ObserveProperty(x => x.ActiveItem);
            Distribution = from ai in activeItem
                           from dist in ai.Distribution
                           select dist; // or just SelectMany
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            ActivateItem(Items[0]);
        }

        public void Check(DistributionViewModel dvm)
        {
            ActivateItem(dvm);
        }

        public void Greet()
        {
            MessageBox.Show("hi");
        }
    }
}