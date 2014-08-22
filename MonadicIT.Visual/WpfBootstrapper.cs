using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using MonadicIT.Common;
using MonadicIT.Visual.Backbone;
using MonadicIT.Visual.ViewModels;

namespace MonadicIT.Visual
{
    public class WpfBootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;

        public WpfBootstrapper()
        {
            Initialize();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void Configure()
        {
            LogManager.GetLog = t => new DebugLog(t);
            MessageBinder.SpecialValues.Add("null", _ => null);
            _container = new SimpleContainer();
            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.PerRequest<ShellViewModel>();
            _container.Singleton<SourceSinkViewModel>();
            _container.Singleton<EntropyCoderViewModel>();
            _container.Singleton<ChannelCoderViewModel>();
            _container.Singleton<ChannelViewModel>();
            _container.Singleton<IChannelCoderDetailViewModel, IdentityCoderViewModel>();
            _container.Singleton<IChannelCoderDetailViewModel, HammingCodeViewModel>();
            _container.Handler<ISourceProperties>(c => c.GetInstance<SourceSinkViewModel>());
            _container.Handler<IEntropyCoderProperties>(c => c.GetInstance<EntropyCoderViewModel>());
            _container.Handler<IChannelCoderProperties>(c => c.GetInstance<ChannelCoderViewModel>());
            _container.Handler<IChannelProperties>(c => c.GetInstance<ChannelViewModel>());
            RegisterDistributionViewModelHandler<Binary>();
            RegisterDistributionViewModelHandler<Ternary>();
            RegisterDistributionViewModelHandler<Common.Decimal>();
            _container.Singleton<TransmissionSystem>();
        }

        private void RegisterDistributionViewModelHandler<T>() where T : /*Enum, */struct
        {
            _container.Handler<DistributionViewModel>(
                _ => new DistributionViewModel(Distribution<T>.Uniform(EnumHelper<T>.Values)));
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}