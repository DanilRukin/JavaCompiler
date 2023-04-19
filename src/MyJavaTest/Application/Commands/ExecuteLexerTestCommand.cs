using JavaCompiler.Common;
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
    public class ExecuteLexerTestCommand
    {
        private Lexer _lexer;
        private IConfiguration _configuration;
        private ILogger _logger;

        public ExecuteLexerTestCommand(Lexer lexer, IConfiguration configuration, ILogger logger)
        {
            _lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Execute(string fileName)
        {
            if (fileName.Substring(fileName.LastIndexOf('.') + 1) == _configuration["LexerTestFileExtension"])
            {
                _lexer.Open(fileName);
                _logger.LogInformation($"\\\\----------------Start testing '{fileName}'-------------------//\r\n");
                Token token;
                while ((token = _lexer.NextToken()).Lexeme != Lexemes.TypeEnd)
                {
                    _logger.LogInformation($"Lexeme type is: {token.Lexeme};\tLexeme value is: {token.Value}\r\n");
                }
                _logger.LogInformation($"Lexeme type is: {token.Lexeme};\tLexeme value is: {token.Value}\r\n");
                _logger.LogInformation($"//----------------Test ends-------------------\\\\\r\n\r\n");
                _lexer.Close();
            }           
        }
    }
}
