using JavaCompiler.LexerAnalyzer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJavaTest.Application.TestRunner;
using System.Runtime.InteropServices.JavaScript;

namespace MyJavaTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IHost host = Host
            .CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddScoped<Lexer>()
                .AddScoped<Runner>();
            })
            .Build();
            try
            {
                host.StartAsync();

                IServiceProvider services = host.Services;
                var runner = services.GetRequiredService<Runner>();
                runner.Run(args);
            }
            catch (Exception e)
            {
                host.Services.GetRequiredService<ILogger>().LogError(e.Message);
            }
            finally
            {
                host.StopAsync();
            }  
        }
    }
}