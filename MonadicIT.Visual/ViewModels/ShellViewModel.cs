using Caliburn.Micro;
using MonadicIT.Visual.Backbone;

namespace MonadicIT.Visual.ViewModels
{
    public sealed class ShellViewModel : Screen
    {
        private readonly TransmissionSystem _system;
        private readonly IWindowManager _windowManager;

        public ShellViewModel(IWindowManager windowManager, SourceSinkViewModel ssvm,
            EntropyCoderViewModel ecvm, ChannelCoderViewModel ccvm, ChannelViewModel cvm, TransmissionSystem system)
        {
            DisplayName = "Information Transmission Simulation";
            _windowManager = windowManager;
            _system = system;
            SourceSink = ssvm;
            EntropyCoder = ecvm;
            ChannelCoder = ccvm;
            Channel = cvm;
        }

        public SourceSinkViewModel SourceSink { get; private set; }

        public EntropyCoderViewModel EntropyCoder { get; private set; }

        public ChannelCoderViewModel ChannelCoder { get; private set; }

        public ChannelViewModel Channel { get; private set; }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            if (close)
            {
                SourceSink.TryClose();
                EntropyCoder.TryClose();
                ChannelCoder.TryClose();
                Channel.TryClose();
            }
        }

        public void ToggleWindow(Screen wnd)
        {
            if (wnd.IsActive) wnd.TryClose();
            else _windowManager.ShowWindow(wnd);
        }
    }
}