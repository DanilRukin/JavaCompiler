using JavaCompiler.Common;
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
    public class ExecuteLexerTestCommand : TestCommand
    {
        private IServiceProvider _serviceProvider;

        public ExecuteLexerTestCommand(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public override void Execute(string fileName)
        {
            IConfiguration configuration = _serviceProvider.GetRequiredService<IConfiguration>();
            ILogger logger = _serviceProvider.GetRequiredService<ILogger>();
            Lexer lexer = _serviceProvider.GetRequiredService<Lexer>();
            if (fileName.Substring(fileName.LastIndexOf('.') + 1) == configuration["LexerTestFileExtension"])
            {
                string text = GetText(fileName);
                lexer.SetText(text);
                logger.LogInformation($"\\\\----------------Start testing '{fileName}'-------------------//\r\n");
                logger.LogInformation($"File content is:\r\n\r\n{text}\r\n\r\n");
                Token token;
                while ((token = lexer.NextToken()).Lexeme != Lexemes.TypeEnd)
                {
                    logger.LogInformation($"Lexeme type is: {token.Lexeme};\tLexeme value is: {token.Value}\r\n");
                }
                logger.LogInformation($"Lexeme type is: {token.Lexeme};\tLexeme value is: {token.Value}\r\n");
                logger.LogInformation($"//----------------Test ends-------------------\\\\\r\n\r\n");
                lexer.ClearText();
            }           
        }
    }
}
