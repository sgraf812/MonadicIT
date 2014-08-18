using System.Windows.Threading;
using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;
using MonadicIT.Source;
using ReactiveUI;

namespace MonadicIT.Visual.ViewModels
{
    public class ShellViewModel : ReactiveConductor<object>.Collection.AllActive 
    {
        private readonly IWindowManager _windowManager;

        public SourceSinkViewModel SourceSink { get; private set; }

        public EntropyCoderViewModel EntropyCoder { get; private set; }

        public ShellViewModel(IWindowManager windowManager, SourceSinkViewModel ssvm, EntropyCoderViewModel ecvm)
        {
            _windowManager = windowManager;
            SourceSink = ssvm;
            EntropyCoder = ecvm;
            Items.Add(SourceSink);
            Items.Add(EntropyCoder);
        }

        public void ToggleWindow(object wnd)
        {
            _windowManager.ShowWindow(wnd);
        }
    }
}