using JavaCompiler.LexerAnalyzer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJavaTest.Application.TestRunner;
using Serilog;
using System.Runtime.InteropServices.JavaScript;

namespace MyJavaTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IHost host = Host
            .CreateDefaultBuilder(args)
            .ConfigureLogging((context, logging) =>
            {
                Log.Logger = new LoggerConfiguration()
                .ReadFrom
                .Configuration(context.Configuration)
                .CreateLogger();
                //logging.AddSerilog();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddScoped<Lexer>()
                .AddScoped<Runner>()
                .AddSingleton(LoggerFactory.Create(logging => logging.AddSerilog()).CreateLogger("Logger"));
            })
            .UseSerilog()
            .Build();
            try
            {
                host.Start();

                IServiceProvider services = host.Services;
                var runner = services.GetRequiredService<Runner>();
                runner.Run(args);
            }
            catch (Exception e)
            {
                host.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger>().LogError(e.Message);
            }
            finally
            {
                host.StopAsync();
            }  
        }
    }
}