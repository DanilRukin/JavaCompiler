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
        [Theory]
        [MemberData(nameof(KeyWordsWithLexemesData))]
        public void NextToken_OnlySimpleKeyWords_ReturnValidTokens(string correctValue, Lexemes correctLexeme)
        {
            Lexer lexer = new Lexer();
            lexer.SetText(correctValue);

            Token token = lexer.NextToken();
            Assert.Equal(correctValue, token.Value);
            Assert.Equal(correctLexeme, token.Lexeme);
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
            };
    }
}
