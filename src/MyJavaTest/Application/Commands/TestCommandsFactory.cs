using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.Application.Commands
{
    public class TestCommandsFactory
    {
        private IServiceProvider _services;
        private IConfiguration _configuration;

        public TestCommandsFactory(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _configuration = _services.GetRequiredService<IConfiguration>();
        }

        public TestCommand? FromExtension(string extension)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));
            else if (extension == _configuration["SyntaxTestFileExtension"])
                return _services.GetRequiredService<ExecuteSyntaxAnalyzerTestCommand>();
            else if (extension == _configuration["LexerTestFileExtension"])
                return _services.GetRequiredService<ExecuteLexerTestCommand>();
            return null;
        }
    }
}
