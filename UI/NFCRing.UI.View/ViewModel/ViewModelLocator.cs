using System;
using System.Linq;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Microsoft.Practices.ServiceLocation;
using NFCRing.UI.View.Services;
using NFCRing.UI.ViewModel;
using NFCRing.UI.ViewModel.Services;
using NLog;

namespace NFCRing.UI.View.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private static IContainer Container { get; set; }

        public MainViewModel MainViewModel => Container.Resolve<MainViewModel>();

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            var builder = new ContainerBuilder();

            RegisterServices(builder);
            RegisterViewModels(builder);

            Container = builder.Build();

            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(Container));
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }

        private void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterInstance(new NLogger()).As<UI.ViewModel.Services.ILogger>();
            builder.RegisterType<DialogService>().As<IDialogService>().SingleInstance();
            builder.RegisterType<TokenService>().As<ITokenService>().SingleInstance();
            builder.RegisterType<SynchronizationService>().As<ISynchronizationService>().SingleInstance();
        }

        private static void RegisterViewModels(ContainerBuilder builder)
        {
            builder.RegisterType<MainViewModel>().SingleInstance();
            builder.Register(x =>
                {
                    var type = typeof(IStepViewModel);
                    var steps = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(t => t.GetTypes())
                        .Where(t => type.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                        .Select(t => Container.Resolve(t) as IStepViewModel)
                        .OrderBy(s => s.Index)
                        .ToList();

                    var wizardViewModel = new WizardViewModel(steps)
                    {
                        CancelAction = () => ServiceLocator.Current.GetInstance<MainViewModel>()
                            .SetContent(ServiceLocator.Current.GetInstance<LoginControlViewModel>())
                    };
                    

                    return wizardViewModel;
                })
                .AsSelf();
            builder.RegisterType<HelloStepViewModel>();
            builder.RegisterType<PlaceRingStepViewModel>();
            builder.RegisterType<RemoveRingStepViewModel>();
            builder.RegisterType<SuccessfullyStepViewModel>();
            builder.RegisterType<LoginStepViewModel>();
            builder.RegisterType<FinishedStepViewModel>();
            builder.RegisterType<LoginControlViewModel>().OnActivated(async x => await x.Instance.InitializeAsync());

            builder.RegisterBuildCallback(x => x.Resolve<MainViewModel>().SetContent(x.Resolve<LoginControlViewModel>()));
        }
    }
}