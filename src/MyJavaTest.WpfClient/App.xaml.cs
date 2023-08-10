using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyJavaTest.WpfClient.Services.Dialogs;
using MyJavaTest.WpfClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MyJavaTest.WpfClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Window FocusedWindow => Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsFocused);

        public static Window ActivedWindow => Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsActive);

        private IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();
        }

        private void ConfigureServices(HostBuilderContext host, IServiceCollection services)
        {
            services.AddTransient<IFolderBrowserDialog, FolderBrowserDialog>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient(
                s =>
                {
                    var scope = s.CreateScope();
                    var model = scope.ServiceProvider.GetRequiredService<MainWindowViewModel>();
                    var window = new MainWindow { DataContext = model };
                    model.DialogComplete += (_, _) => window.Close();
                    window.Closed += (_, _) => scope.Dispose();

                    return window;
                });
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await _host.StartAsync();

            var window = _host.Services.GetRequiredService<MainWindow>();
            window.Show();

        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            using (_host) await _host.StopAsync();
        }
    }
}
