using System;
using System.Reactive.Linq;
using MonadicIT.Channel;
using MonadicIT.Common;

namespace MonadicIT.Visual.ViewModels
{
    public class IdentityCoderViewModel : SelectableViewModelBase, IChannelCoderDetailViewModel
    {
        public IdentityCoderViewModel()
        {
            ChannelCoder = Observable.Repeat(new IdentityCoder(), 1);
        }

        public override string DisplayName
        {
            get { return "No channel coding"; }
        }

        public IObservable<IChannelCoder<Binary>> ChannelCoder { get; private set; }
    }
}