using JavaCompiler.LexerAnalyzer;
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
    public class ExecuteTestsInsideDirCommand
    {
        private IServiceProvider _services;

        public ExecuteTestsInsideDirCommand(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public void Execute(string directoryName)
        {
            ILogger logger = _services.GetRequiredService<ILogger>();
            IConfiguration configuration = _services.GetRequiredService<IConfiguration>();
            if (!Directory.Exists(directoryName))
                logger.LogError($"No such directory with name: {directoryName}");
            else
            {
                string[] files = Directory.GetFiles(directoryName);
                ExecuteLexerTestCommand lexerTestCommand = _services.GetRequiredService<ExecuteLexerTestCommand>();
                ExecuteSyntaxAnalyzerTestCommand syntaxAnalyzerTestCommand = _services.GetRequiredService<ExecuteSyntaxAnalyzerTestCommand>();
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Substring(files[i].LastIndexOf('.') + 1) == configuration["SyntaxTestFileExtension"])
                        syntaxAnalyzerTestCommand.Execute(files[i]);
                    else if (files[i].Substring(files[i].LastIndexOf('.') + 1) == configuration["LexerTestFileExtension"])
                        lexerTestCommand.Execute(files[i]);
                    else
                        logger.LogWarning($"Unknown file: '{files[i]}'\r\n");
                }
            }
        }
    }
}
