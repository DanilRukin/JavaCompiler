using JavaCompiler.Common;
using JavaCompiler.LexerAnalyzer;
using JavaCompiler.SyntaxAnalyzer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.Application.Commands
{
    public class ExecuteSyntaxAnalyzerTestCommand : TestCommand
    {
        private IServiceProvider _services;

        public ExecuteSyntaxAnalyzerTestCommand(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public override void Execute(string fileName)
        {
            IConfiguration configuration = _services.GetRequiredService<IConfiguration>();
            ILogger logger = _services.GetRequiredService<ILogger>();
            SyntaxAnalyzer syntaxAnalyzer = _services.GetRequiredService<SyntaxAnalyzer>();
            if (fileName.Substring(fileName.LastIndexOf('.') + 1) == configuration["SyntaxTestFileExtension"])
            {
                string text = GetText(fileName);
                syntaxAnalyzer.SetText(text);
                logger.LogInformation($"\\\\----------------Start testing '{fileName}'-------------------//\r\n");
                logger.LogInformation($"File content is:\r\n\r\n{text}\r\n\r\n");
                try
                {
                    syntaxAnalyzer.Analyze();
                    logger.LogInformation("No errors detected...\r\n");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                }
                logger.LogInformation($"//----------------Test ends-------------------\\\\\r\n\r\n");
                syntaxAnalyzer.ClearText();
            }
        }
    }
}
