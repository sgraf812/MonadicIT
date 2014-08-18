using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using MonadicIT.Common;
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
            _container.PerRequest<ShellViewModel>();
            _container.PerRequest<SourceSinkViewModel>();
            //_container.Singleton<DistributionViewModel>();
            RegisterDistributionVMInstance<Binary>();
            RegisterDistributionVMInstance<Ternary>();
            RegisterDistributionVMInstance<Common.Decimal>();
        }

        private void RegisterDistributionVMInstance<T>() where T : /*Enum, */struct
        {
            _container.Instance(new DistributionViewModel(Distribution<T>.Uniform(EnumHelper<T>.Values)));
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}