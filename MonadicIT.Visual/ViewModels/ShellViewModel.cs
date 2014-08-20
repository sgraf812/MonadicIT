using Caliburn.Micro;

namespace MonadicIT.Visual.ViewModels
{
    public class ShellViewModel : Screen
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
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            if (close)
            {
                SourceSink.TryClose();
                EntropyCoder.TryClose();
                ChannelCoder.TryClose(); 
            }
        }

        public void ToggleWindow(Screen wnd)
        {
            if (wnd.IsActive) wnd.TryClose();
            else _windowManager.ShowWindow(wnd);
        }
    }
}