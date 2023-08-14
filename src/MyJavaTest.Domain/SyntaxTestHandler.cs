using JavaCompiler.SyntaxAnalyzer;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.Domain
{
    internal class SyntaxTestHandler : IRequestHandler<SyntaxTest>
    {
        private readonly ILogger<SyntaxTestHandler> _logger;
        private readonly SyntaxAnalyzer _syntaxAnalyzer;

        public SyntaxTestHandler(ILogger<SyntaxTestHandler> logger, SyntaxAnalyzer syntaxAnalyzer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _syntaxAnalyzer = syntaxAnalyzer ?? throw new ArgumentNullException(nameof(syntaxAnalyzer));
        }

        public Task Handle(SyntaxTest request, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                string text = request.Content;
                _syntaxAnalyzer.SetText(text);
                _logger.LogInformation($"\\\\----------------Start testing '{request.FileName}'-------------------//\r\n");
                _logger.LogInformation($"File content is:\r\n\r\n{text}\r\n\r\n");
                try
                {
                    _syntaxAnalyzer.Analyze();
                    _logger.LogInformation("No errors detected...\r\n");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                _logger.LogInformation($"//----------------Test ends-------------------\\\\\r\n\r\n");
                _syntaxAnalyzer.ClearText();
            }, cancellationToken);           
        }
    }
}
