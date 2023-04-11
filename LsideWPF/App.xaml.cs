using LsideWPF.Services;
using LsideWPF.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace LsideWPF
{
    public partial class App : Application
    {

        public new static App Current => (App)Application.Current;

        public App()
        {
            Services = ConfigureServices();
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetupExceptionHandling();

            MainWindow window = new MainWindow();
            window.Show();
        }


        // IOC provider
        public IServiceProvider Services { get; }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ILandingLoggerService, LandingLogger>();
            services.AddSingleton<ISimService, SimService>();

            return services.BuildServiceProvider();
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private void LogUnhandledException(Exception e, string source)
        {
            MessageBox.Show(e.Message);
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string logout = "\n\n" + DateTime.Now.ToString() + "\n" + System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion + "\n" +
                e.Message + "\n" + e.Source + "\n" + e.StackTrace;
            System.IO.File.AppendAllText(@"./log.txt", logout);
        }
    }
}
