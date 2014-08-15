using System.Windows;
using Caliburn.Micro;

namespace MonadicIT.Visual
{
    public class WpfBootstrapper : BootstrapperBase
    {
        public WpfBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}