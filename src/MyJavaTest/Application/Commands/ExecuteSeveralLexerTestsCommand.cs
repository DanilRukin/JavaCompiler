using JavaCompiler.LexerAnalyzer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.Application.Commands
{
    public class ExecuteSeveralLexerTestsCommand
    {
        private ExecuteLexerTestCommand _command;
        private Lexer _lexer;
        private IConfiguration _configuration;
        private ILogger _logger;

        public ExecuteSeveralLexerTestsCommand(Lexer lexer, IConfiguration configuration, ILogger logger)
        {
            _lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Execute(string directoryName)
        {
            if (!Directory.Exists(directoryName))
                _logger.LogCritical($"No such directory with name: {directoryName}");
            else
            {
                _command = new(_lexer, _configuration, _logger);
                string[] files = Directory.GetFiles(directoryName);
                for (int i = 0; i < files.Length; i++)
                {
                    _command.Execute(files[i]);
                }
            }
        }
    }
}
