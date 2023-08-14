using JavaCompiler.Common;
using JavaCompiler.LexerAnalyzer;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MyJavaTest.Domain
{
    internal class LexerTestHandler : IRequestHandler<LexerTest>
    {
        private readonly ILogger<LexerTestHandler> _logger;
        private readonly Lexer _lexer;
        internal LexerTestHandler(ILogger<LexerTestHandler> logger, Lexer lexer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
        }
        public Task Handle(LexerTest test, CancellationToken cancellationToken = default)
        {
            return Task.Factory.StartNew(() =>
            {
                string text = test.Content;
                _lexer.SetText(text);
                _logger.LogInformation($"\\\\----------------Start testing '{test.FileName}'-------------------//\r\n");
                _logger.LogInformation($"File content is:\r\n\r\n{text}\r\n\r\n");
                Token token;
                while ((token = _lexer.NextToken()).Lexeme != Lexemes.TypeEnd)
                {
                    _logger.LogInformation($"Lexeme type is: {token.Lexeme};\tLexeme value is: {token.Value}\r\n");
                }
                _logger.LogInformation($"Lexeme type is: {token.Lexeme};\tLexeme value is: {token.Value}\r\n");
                _logger.LogInformation($"//----------------Test ends-------------------\\\\\r\n\r\n");
                _lexer.ClearText();
            }, cancellationToken);
        }
    }
}
