using JavaCompiler.LexerAnalyzer;
using JavaCompiler.SyntaxAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.JavaCompiler
{
    public class SyntaxAnalyzerTests
    {
        private SyntaxAnalyzer _analyzer;

        public SyntaxAnalyzerTests()
        {
            _analyzer = new SyntaxAnalyzer();
        }

        [Fact]
        public void Analyze_SimpleClassDeclaration_NoClassName_ThrowsSyntaxErrorExceptionWithMessage()
        {
            string text = "class { }";
            string message = "Ожидался идентификатор, но отсканирован символ '{'." +
                $"Строка: 0, столбец: {text.IndexOf("{")}";
            _analyzer.SetText(text);
            var error = Assert.Throws<SyntaxErrorException>(_analyzer.Analyze);
            Assert.NotNull(error);
            Assert.Equal(message, error.Message);
            _analyzer.ClearText();
        }
    }
}
