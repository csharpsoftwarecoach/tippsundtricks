using Presentation.Interfaces;
using Presentation.Ioc;
using System.Windows;
using ViewModels;
using Views;

namespace ApplicationProject
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ConfigureServices(InversionOfControlContainer.Instance);

            Application.Current.Resources.Add(InversionOfControlKey.IocWpfContainer, InversionOfControlContainer.Instance);

            MainWindowView mainWindowView = new MainWindowView();
            mainWindowView.Show();
        }

        private static void ConfigureServices(InversionOfControlContainer iocContainer)
        {
            iocContainer.RegisterViewModel<IViewModel, MainWindowViewModel>();
        }
    }
}
