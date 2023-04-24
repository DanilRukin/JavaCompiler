using JavaCompiler.Common;
using JavaCompiler.LexerAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.JavaCompiler
{
    public class LexerTests
    {
        Lexer _lexer;

        public LexerTests() => _lexer = new Lexer();

        [Theory]
        [MemberData(nameof(KeyWordsWithLexemesData))]
        public void NextToken_OnlySimpleKeyWords_ReturnValidTokens(string correctValue, Lexemes correctLexeme)
        {
            _lexer.SetText(correctValue);

            Token token = _lexer.NextToken();
            Assert.Equal(correctValue, token.Value);
            Assert.Equal(correctLexeme, token.Lexeme);

            _lexer.ClearText();
        }

        public static IEnumerable<object[]> KeyWordsWithLexemesData =>
            new List<object[]>
            {
                new object[] { "class", Lexemes.TypeClassKeyWord },
                new object[] { "while", Lexemes.TypeWhileKeyWord },
                new object[] { "new", Lexemes.TypeNewKeyWord },
                new object[] { "return", Lexemes.TypeReturnKeyWord },
                new object[] { "int", Lexemes.TypeIntKeyWord },
                new object[] { "double", Lexemes.TypeDoubleKeyWord },
                new object[] { "boolean", Lexemes.TypeBooleanKeyWord },
                new object[] { "final", Lexemes.TypeFinalKeyWord },
                new object[] { "true", Lexemes.TypeBooleanLiteral },
                new object[] { "false", Lexemes.TypeBooleanLiteral },
                new object[] { "null", Lexemes.TypeNullLiteral },
                new object[] { ">", Lexemes.TypeMoreSign },
                new object[] { ">=", Lexemes.TypeMoreOrEqualSign },
                new object[] { "<", Lexemes.TypeLessSign },
                new object[] { "<=", Lexemes.TypeLessOrEqualSign },
                new object[] { "=", Lexemes.TypeAssignmentSign },
                new object[] { "==", Lexemes.TypeEqualSign },
                new object[] { "!=", Lexemes.TypeNotEqualSign },
                new object[] { "*", Lexemes.TypeMult },
                new object[] { "/", Lexemes.TypeDiv },
                new object[] { "%", Lexemes.TypeMod },
                new object[] { ".", Lexemes.TypeDot },
                new object[] { ";", Lexemes.TypeSemicolon },
                new object[] { ",", Lexemes.TypeComma },
                new object[] { "{", Lexemes.TypeOpenCurlyBrace },
                new object[] { "}", Lexemes.TypeCloseCurlyBrace },
                new object[] { "(", Lexemes.TypeOpenParenthesis },
                new object[] { ")", Lexemes.TypeCloseParenthesis },
                new object[] { "+", Lexemes.TypePlus },
                new object[] { "++", Lexemes.TypeIncrement },
                new object[] { "-", Lexemes.TypeMinus },
                new object[] { "--", Lexemes.TypeDecrement },
                new object[] { "void", Lexemes.TypeVoidKeyWord },
            };


        [Theory]
        [InlineData("// simple comment")]
        [InlineData("// // /12345!@#$/")]
        [InlineData("// class main {} %")]
        [InlineData("// void static hash")]
        [InlineData("// /**/")]
        [InlineData("// /*******///")]
        public void NextToken_SimpleComments_ReturnDefaultToken(string text)
        {
            _lexer.SetText(text);
            Token token = _lexer.NextToken();
            Token def = Token.Default();

            //Assert.Equal(def.Lexeme, token.Lexeme);
            //Assert.Equal(def.Value, token.Value);

            Assert.Equal(def, token);

            _lexer.ClearText();
        }

        [Theory]
        [InlineData("/**/")]
        [InlineData("/***/")]
        [InlineData("/****/")]
        [InlineData("/**hello*,world*/")]
        [InlineData("/**hello*,world*\r\nsome text class Main\r\n/")] // думаю, что можно считать допустимым такой комментарий, т.к. после него никакого кода
        [InlineData("/**hello*,world*\r\nsome text class Main\r\n/some text")]
        public void NextToken_MultilineComments_ReturnDefaultToken(string text)
        {
            _lexer.SetText(text);

            Token token = _lexer.NextToken();
            Token def = Token.Default();

            Assert.Equal(def, token);

            _lexer.ClearText();
        }

        [Theory]
        [MemberData(nameof(CommentTextAndToken))]
        public void NextToken_MultilineCommnets_TextAfterEndOfComment_ReturnValidToken(string comment, string text, Token validToken)
        {
            _lexer.SetText(string.Concat(comment, text));

            Token token = _lexer.NextToken();

            Assert.Equal(validToken, token);

            _lexer.ClearText();
        }

        public static IEnumerable<object[]> CommentTextAndToken =>
            new List<object[]>
            {
                new object[] {"/*comment*/", "class", new Token(Lexemes.TypeClassKeyWord, "class") },
                new object[] {"/*comment*/", "final", new Token(Lexemes.TypeFinalKeyWord, "final") },
                new object[] {"/*comment*/", "Main", new Token(Lexemes.TypeIdentifier, "Main") },
                new object[] {"/*comment*/", "new", new Token(Lexemes.TypeNewKeyWord, "new") },
                new object[] {"/*comment*/", "void", new Token(Lexemes.TypeVoidKeyWord, "void") },
                new object[] {"/*comment*/", "45", new Token(Lexemes.TypeIntLiteral, "45") },
                new object[] {"/*comment*/", "int", new Token(Lexemes.TypeIntKeyWord, "int") },
                new object[] {"/*comment*/", "125.3", new Token(Lexemes.TypeDoubleLiteral, "125.3") },
                new object[] {"/*comment*/", "true", new Token(Lexemes.TypeBooleanLiteral, "true") },
                new object[] {"/*comment*/", "null", new Token(Lexemes.TypeNullLiteral, "null") },
                new object[] {"/*comment*/", "double", new Token(Lexemes.TypeDoubleKeyWord, "double") },
            };

        [Fact]
        public void SavePosition_RestorePosition_TwoTokensInText_SavePositionAfterFirstTokenAndRestoreAfterSecond_ReturnSecondToken()
        {
            string text = "class Main";

            _lexer.SetText(text);

            Token token = _lexer.NextToken();
            Lexer.Position position = _lexer.SavePosition();
            token = _lexer.NextToken();
            Assert.Equal("Main", token.Value);

            token = _lexer.NextToken();
            Assert.Equal(Token.Default(), token);

            _lexer.RestorePosition(position);

            token = _lexer.NextToken();

            Assert.Equal("Main", token.Value);

            _lexer.ClearText();
        }
    }
}
