using Caliburn.Micro;

namespace MonadicIT.Visual.ViewModels
{
    public class ShellViewModel : Conductor<object>.Collection.AllActive 
    {
        private readonly IWindowManager _windowManager;

        public SourceSinkViewModel SourceSink { get; private set; }

        public EntropyCoderViewModel EntropyCoder { get; private set; }

        public ChannelCoderViewModel ChannelCoder { get; private set; }

        public ChannelViewModel Channel { get; private set; }

        public ShellViewModel(IWindowManager windowManager, SourceSinkViewModel ssvm, 
            EntropyCoderViewModel ecvm, ChannelCoderViewModel ccvm, ChannelViewModel cvm)
        {
            _windowManager = windowManager;
            SourceSink = ssvm;
            EntropyCoder = ecvm;
            ChannelCoder = ccvm;
            Channel = cvm;
            Items.Add(SourceSink);
            Items.Add(EntropyCoder);
            Items.Add(Channel);
            Items.Add(ChannelCoder);
        }

        public void ToggleWindow(object wnd)
        {
            _windowManager.ShowWindow(wnd);
        }
    }
}