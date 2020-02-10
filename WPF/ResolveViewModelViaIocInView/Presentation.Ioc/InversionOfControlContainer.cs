using System;
using System.Linq;
using Unity;
using Unity.Resolution;

namespace Presentation.Ioc
{
    public class InversionOfControlContainer
    {
        private IUnityContainer _iocContainer;

        private static InversionOfControlContainer _instance = null;

        private static readonly object _lockInstance = new object();

        public InversionOfControlContainer()
        {
            _iocContainer = new UnityContainer();
        }

        public static InversionOfControlContainer Instance
        {
            get
            {
                lock (_lockInstance)
                {
                    if (_instance == null)
                    {
                        _instance = new InversionOfControlContainer();
                    }

                    return _instance;
                }
            }
        }

        public T Resolve<T>(string name, params Tuple<string, object>[] parameter)
        {
            return _iocContainer.Resolve<T>(name, parameter.Select(x => new ParameterOverride(x.Item1, x.Item2)).ToArray());
        }

        public bool IsRegistered<T>(string name)
        {
            return _iocContainer.IsRegistered<T>(name);
        }

        public void RegisterViewModel<TAbstraction, TImplementation>() where TImplementation : TAbstraction
        {
            _iocContainer.RegisterType<TAbstraction, TImplementation>(typeof(TImplementation).Name);
        }
    }
}
