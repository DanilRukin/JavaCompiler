namespace MyJavaTest.WebClient.Models
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
