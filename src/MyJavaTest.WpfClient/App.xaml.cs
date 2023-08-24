using JavaCompiler.LexerAnalyzer;
using JavaCompiler.SyntaxAnalyzer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyJavaTest.WpfClient.Models.Services;
using MyJavaTest.WpfClient.Models.Services.Interfaces;
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
            services.AddTransient<Lexer>();
            services.AddTransient<SyntaxAnalyzer>();
            services.AddTransient<ITestFactory, TestFactory>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient(
                s =>
                {
                    var scope = s.CreateScope();
                    var model = scope.ServiceProvider.GetRequiredService<MainWindowViewModel>();
                    //Lexer lexer = scope.ServiceProvider.GetRequiredService<Lexer>();
                    //SyntaxAnalyzer syntaxAnalyzer = scope.ServiceProvider.GetRequiredService<SyntaxAnalyzer>();
                    //ITestFactory testFactory = scope.ServiceProvider.GetRequiredService<ITestFactory>();
                    //IFolderBrowserDialog dialog = scope.ServiceProvider.GetRequiredService<IFolderBrowserDialog>();
                    var window = new MainWindow()
                    {
                        DataContext = model
                    };
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
