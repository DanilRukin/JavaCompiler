using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.WpfClient.Models
{
    public enum TestFileType
    {
        LexerTest, SyntaxTest, SemanticTest, Unknown
    }

    public static class TestFileTypeExtensions
    {
        public static string GetName(this TestFileType type) =>
            type switch
            {
                TestFileType.SemanticTest => "Семантический",
                TestFileType.LexerTest => "Лексический",
                TestFileType.SyntaxTest => "Синтаксический",
                TestFileType.Unknown => "Неизвестно",
                _ => string.Empty
            };
    }
}
