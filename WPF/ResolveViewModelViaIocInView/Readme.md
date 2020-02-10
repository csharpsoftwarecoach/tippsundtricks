# Überblick

Dieses Projekt zeigt wie man mithilfe eines IoC-Containers und einer Markup-Extension eine 100 %
lose Koppelung zwischen View und ViewModel erreichen kann.
 
Der IoC-Container (in diesem Fall Unity) wurde von mir nochmals weggekapselt, damit die Unity-Reference in einer Assembly vorhanden ist,
somit lässt sich das Framework des Containers leicht austauschen.

# Assemblies
* ApplicationProject (Startprojekt)
* Views
* ViewModels
* Presentation.Interfaces (IViewModel)
* Presentation.Ioc (der Wrapper für den Ioc-Container - mit Unity)
* Presentation.Wpf.Ioc (MarkupExtension, welche den DataContext über Ioc setzt)


### ApplicationProject.App.xaml.cs
An dieser Stelle wird das MainWindowViewModel über das Interface registriert.
Zusätzlich wird der Ioc-Container in die Application.Resource geladen.
Die MarkupExtension kann dann über die Ressource den IoC-Container verwenden.
```csharp
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
```

### Presentation.Wpf.Ioc.IocMarkupExtension.cs
In diesem Codeabschnitt aus der MarkupExtension wird das ViewModel über IoC aufgelöst.
```csharp
                if (Application.Current.Resources.Contains(InversionOfControlKey.IocWpfContainer))
                {
                    var container = Application.Current.Resources[InversionOfControlKey.IocWpfContainer] as InversionOfControlContainer;
                    if (container != null)
                    {
                        if (!string.IsNullOrEmpty(this.ViewModel) && container.IsRegistered<IViewModel>(this.ViewModel))
                        {
                            return container.Resolve<IViewModel>(this.ViewModel, new Tuple<string, object>("iocContainer", container));
                        }
                        else if (string.IsNullOrEmpty(this.ViewModel))
                        {
                            var key = frameworkElement.GetType().Name + "Model"; 
                            if (container.IsRegistered<IViewModel>(key))
                            {
                                return container.Resolve<IViewModel>(key, new Tuple<string, object>("iocContainer", container));
                            }
                        }
                    }
                }
```

### Views.MainWindowView.xaml
In der View wird der DataContext über die MarkupExtension gesetzt (hier gibt man das entsprechende ViewModel an).
```xaml
<Window x:Class="Views.MainWindowView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:ioc="clr-namespace:Presentation.Wpf.Ioc;assembly=Presentation.Wpf.Ioc"
        DataContext="{ioc:IocMarkupExtension ViewModel=MainWindowViewModel}"
        mc:Ignorable="d"
        Title="MainWindow" Height="450" Width="800">
    <Grid>
        <Label Content="{Binding DisplayName}" VerticalAlignment="Center" HorizontalAlignment="Center" Foreground="Red" />
    </Grid>
</Window>
```
