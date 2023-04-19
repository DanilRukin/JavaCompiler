using JavaCompiler.LexerAnalyzer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyJavaTest.Application.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.Application.TestRunner
{
    public class Runner
    {
        private IConfiguration _configuration;
        private ILogger _logger;
        private Lexer _lexer;

        public Runner(Lexer lexer, IConfiguration configuration, ILogger logger)
        {
            _lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Run(string[] commandLineArgs)
        {
            if (commandLineArgs.Length == 0)
            {
                _logger.LogWarning("No arguments were passed\r\n");
                return;
            }

            string command = commandLineArgs[0];
            switch (command)
            {
                case Commands.RunFileCommand:
                    {
                        if (commandLineArgs.Length > 2 || commandLineArgs.Length == 1)
                            _logger.LogError($"Invalid parameters count for command '{Commands.RunFileCommand}'. Count is {commandLineArgs.Length - 1}. " +
                                $"Use '{Commands.RunFilesCommand}' instead.\r\n");
                        else
                        {
                            var lexerCommand = new ExecuteLexerTestCommand(_lexer, _configuration, _logger);
                            lexerCommand.Execute(commandLineArgs[1]);
                        }
                        break;
                    }
                case Commands.RunFilesCommand:
                    {
                        if (commandLineArgs.Length == 1)
                            _logger.LogError($"Invalid parameters count for command '{Commands.RunFilesCommand}'. Count is {commandLineArgs.Length - 1}.\r\n");
                        else
                        {
                            var lexerCommand = new ExecuteLexerTestCommand(_lexer, _configuration, _logger);
                            for (int i = 1; i < commandLineArgs.Length; i++)
                            {
                                lexerCommand.Execute(commandLineArgs[i]);
                            }
                        }
                        break;
                    }
                case Commands.RunDirCommand:
                    {
                        if (commandLineArgs.Length > 2)
                            _logger.LogError($"Invalid parameters count for command '{Commands.RunDirCommand}'. Count is {commandLineArgs.Length - 1}.\r\n");
                        else if (commandLineArgs.Length == 1)
                        {
                            string currentDir = Directory.GetCurrentDirectory();
                            _logger.LogInformation($"Reading current directory '{currentDir}' to find test files...\r\n");
                            var lexerCommand = new ExecuteSeveralLexerTestsCommand(_lexer, _configuration, _logger);
                            lexerCommand.Execute(currentDir);
                        }
                        else
                        {
                            var lexerCommand = new ExecuteSeveralLexerTestsCommand(_lexer, _configuration, _logger);
                            lexerCommand.Execute(commandLineArgs[1]);
                        }
                        break;
                    }
                default:
                    _logger.LogError($"Invalid command: '{command}'");
                    break;
            }
        }
    }
}
