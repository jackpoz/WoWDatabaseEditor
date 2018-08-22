using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using Unity.RegistrationByConvention;
using WDE.Common;
using WDE.Common.Managers;
using WDE.Common.Services;
using WDE.Common.Windows;
using WDE.DbcStore;
using WDE.History;
using WDE.HistoryWindow;
using WDE.MySqlDatabase;
using WDE.Parameters;
using WDE.SmartScriptEditor;
using WDE.Solutions;
using WDE.Solutions.Manager;
using WDE.SQLEditor;
using WoWDatabaseEditor.Events;
using WoWDatabaseEditor.Managers;
using WoWDatabaseEditor.Services.ConfigurationService;
using WoWDatabaseEditor.Services.NewItemService;
using WoWDatabaseEditor.Views;

namespace WoWDatabaseEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            //Container.GetContainer().RegisterType<IEventAggregator, EventAggregator>(new ContainerControlledLifetimeManager());
            //Container.GetContainer().RegisterType<IWindowManager, WindowManager>(new ContainerControlledLifetimeManager());
            //Container.GetContainer().RegisterType<ISolutionEditorManager, SolutionEditorManager>(new ContainerControlledLifetimeManager());
            //Container.GetContainer().RegisterType<IWindowProvider, SolutionEditorManager>("SolutionExplorer");
           // Container.GetContainer().RegisterTypes(AllClasses.FromAssemblies(typeof(HistoryModule).Assembly), (c) => WithMappings.FromMatchingInterface(c)); ;

            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<IWindowManager, WindowManager>();
            containerRegistry.RegisterSingleton<ISolutionEditorManager, SolutionEditorManager>();
            containerRegistry.Register<IWindowProvider, SolutionEditorManager>("SolutionExplorer");

            containerRegistry.Register<IConfigureService, ConfigureService>();
            containerRegistry.Register<INewItemService, NewItemService>();
            containerRegistry.Register<INewItemWindowViewModel, NewItemWindowViewModel>();
            containerRegistry.Register<ISolutionManager, SolutionManager>();

            

        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
            //moduleCatalog.AddModule(typeof(HistoryModule));
            moduleCatalog.AddModule(typeof(MySqlDatabaseModule));

            moduleCatalog.AddModule(typeof(HistoryModule));
            moduleCatalog.AddModule(typeof(ParametersModule));
            moduleCatalog.AddModule(typeof(DbcStoreModule));
            moduleCatalog.AddModule(typeof(SmartScriptModule));
            moduleCatalog.AddModule(typeof(SqlEditorModule));
            moduleCatalog.AddModule(typeof(SolutionsModule));

            moduleCatalog.AddModule(typeof(HistoryWindowModule));

            var eventAggregator = Container.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<AllModulesLoaded>().Publish();
        }
        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);

        //    var bootstrapper = new Bootstrapper();
        //    bootstrapper.Run();
        //}
    }
}
