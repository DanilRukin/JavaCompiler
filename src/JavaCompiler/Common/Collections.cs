using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaCompiler.Common
{
    public class Collections
    {
        public static string[] KeyWords = new string[]
        {
            "class", "while", "new", "return", "int", "double", "boolean", "final"
        };

        public static string[] Literals = new string[]
        {
            "true", "false", "null",
        };

        public static string[] Comments = new string[]
        {
            "/*", "//"
        };

        public static Dictionary<string, Lexemes> Words = new Dictionary<string, Lexemes>()
        {
            { "class", Lexemes.TypeClassKeyWord },
            { "while", Lexemes.TypeWhileKeyWord },
            { "new", Lexemes.TypeNewKeyWord },
            { "return", Lexemes.TypeReturnKeyWord },
            { "int", Lexemes.TypeIntKeyWord },
            { "double", Lexemes.TypeDoubleKeyWord },
            { "boolean", Lexemes.TypeBooleanKeyWord },
            { "final", Lexemes.TypeFinalKeyWord },
            { "true", Lexemes.TypeBooleanLiteral },
            { "false", Lexemes.TypeBooleanLiteral },
            { "null", Lexemes.TypeNullLiteral },
            { ">", Lexemes.TypeMoreSign },
            { ">=", Lexemes.TypeMoreOrEqualSign },
            { "<", Lexemes.TypeLessSign },
            { "<=", Lexemes.TypeLessOrEqualSign },
            { "=", Lexemes.TypeAssignmentSign },
            { "==", Lexemes.TypeEqualSign },
            { "!=", Lexemes.TypeNotEqualSign },
            { "*", Lexemes.TypeMult },
            { "/", Lexemes.TypeDiv },
            { "%", Lexemes.TypeMod },
            { ".", Lexemes.TypeDot },
            { ";", Lexemes.TypeSemicolon },
            { ",", Lexemes.TypeComma },
            { "{", Lexemes.TypeOpenCurlyBrace },
            { "}", Lexemes.TypeCloseCurlyBrace },
            { "(", Lexemes.TypeOpenParenthesis },
            { ")", Lexemes.TypeCloseParenthesis },
            { "+", Lexemes.TypePlus },
            { "++", Lexemes.TypeIncrement },
            { "-", Lexemes.TypeMinus },
            { "--", Lexemes.TypeDecrement },
        };

        public static Lexemes GetLexemeByName(string name)
        {
            bool success = Words.TryGetValue(name, out Lexemes lexeme);
            if (!success)
                lexeme = Lexemes.TypeIdentifier;
            return lexeme;
        }
    }
}
