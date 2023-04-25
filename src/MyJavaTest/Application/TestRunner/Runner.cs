using JavaCompiler.LexerAnalyzer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        private IServiceProvider _services;
        private ILogger _logger;
        private TestCommandsFactory _commandsFactory;

        public Runner(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _logger = _services.GetRequiredService<ILogger>();
            _commandsFactory = _services.GetRequiredService<TestCommandsFactory>();
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
                            _commandsFactory.FromExtension(Extension(commandLineArgs[1]))?.Execute(commandLineArgs[1]);
                        }
                        break;
                    }
                case Commands.RunFilesCommand:
                    {
                        if (commandLineArgs.Length == 1)
                            _logger.LogError($"Invalid parameters count for command '{Commands.RunFilesCommand}'. Count is {commandLineArgs.Length - 1}.\r\n");
                        else
                        {
                            for (int i = 1; i < commandLineArgs.Length; i++)
                            {
                                _commandsFactory
                                    .FromExtension(Extension(commandLineArgs[i]))
                                    ?.Execute(commandLineArgs[i]);
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
                            var testCommand = _services.GetRequiredService<ExecuteTestsInsideDirCommand>();
                            testCommand.Execute(currentDir);
                        }
                        else
                        {
                            var testCommand = _services.GetRequiredService<ExecuteTestsInsideDirCommand>();
                            testCommand.Execute(commandLineArgs[1]);
                        }
                        break;
                    }
                default:
                    _logger.LogError($"Invalid command: '{command}'");
                    break;
            }
        }

        private string Extension(string fileName) => fileName.Substring(fileName.LastIndexOf(".") + 1);
    }
}
