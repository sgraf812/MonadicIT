using Caliburn.Micro;

namespace MonadicIT.Visual.ViewModels
{
    public class ShellViewModel : Conductor<object>.Collection.AllActive 
    {
        private readonly IWindowManager _windowManager;

        public SourceSinkViewModel SourceSink { get; private set; }

        public EntropyCoderViewModel EntropyCoder { get; private set; }

        public ChannelViewModel Channel { get; private set; }

        public ShellViewModel(IWindowManager windowManager, SourceSinkViewModel ssvm, EntropyCoderViewModel ecvm, ChannelViewModel cvm)
        {
            _windowManager = windowManager;
            SourceSink = ssvm;
            EntropyCoder = ecvm;
            Channel = cvm;
            Items.Add(SourceSink);
            Items.Add(EntropyCoder);
            Items.Add(Channel);
        }

        public void ToggleWindow(object wnd)
        {
            _windowManager.ShowWindow(wnd);
        }
    }
}