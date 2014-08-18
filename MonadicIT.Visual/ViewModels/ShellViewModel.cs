using System.Windows.Threading;
using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using MonadicIT.Common;

namespace MonadicIT.Visual.ViewModels
{
    public class ShellViewModel : ReactiveConductor<object>.Collection.AllActive 
    {
        private readonly IWindowManager _windowManager;
        private readonly DistributionViewModel _dist = new DistributionViewModel(Distribution<Common.Decimal>.Uniform(EnumHelper<Decimal>.Values));
        public SourceSinkViewModel SourceSink { get; set; }
        public DistributionViewModel Distribution { get { return _dist; } }

        public ShellViewModel(IWindowManager windowManager, SourceSinkViewModel ssvm)
        {
            _windowManager = windowManager;
            SourceSink = ssvm;
        }

        public void ToggleWindow(object wnd)
        {
            _windowManager.ShowWindow(wnd);
        }
    }
}