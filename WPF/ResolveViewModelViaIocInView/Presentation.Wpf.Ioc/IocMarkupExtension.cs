using Presentation.Interfaces;
using Presentation.Ioc;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using System.Xaml;

namespace Presentation.Wpf.Ioc
{
    public class IocMarkupExtension : MarkupExtension
    {
        public string ViewModel { get; set; }

        public IocMarkupExtension()
        {
        }

        public IocMarkupExtension(string viewModel)
        {
            this.ViewModel = viewModel;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return new object();
            }

            var frameworkElement = (serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider)?.RootObject as FrameworkElement;
            if (frameworkElement != null)
            {
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
            }

            return DependencyProperty.UnsetValue;
        }
    }
}
