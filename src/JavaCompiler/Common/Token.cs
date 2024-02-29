using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaCompiler.Common
{
    public class Token
    {
        public Lexemes Lexeme { get; set; }
        public string Value { get; set; }
        public Token(Lexemes lexeme, string value)
        {
            Lexeme = lexeme;
            Value = value;
        }
        /// <summary>
        /// Устанавливает значение токена по-умолчанию.
        /// Значение по умолчанию: конец текста.
        /// </summary>
        /// <returns></returns>
        public static Token Default() => new Token(Lexemes.TypeEnd, "#");

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;
            if (!(obj is Token)) 
                return false;
            Token token = (Token)obj;
            return token.Lexeme == this.Lexeme && token.Value == this.Value;
        }

        public static Token FromLexeme(Lexemes lexeme)
        {
            Token result = new Token(lexeme, "");
            string lexemeValue = "";
            switch (lexeme)
            {
                case Lexemes.TypeClassKeyWord:
                    lexemeValue = "class";
                    break;
                case Lexemes.TypeWhileKeyWord:
                    lexemeValue = "while";
                    break;
                case Lexemes.TypeReturnKeyWord:
                    lexemeValue = "return";
                    break;
                case Lexemes.TypeFinalKeyWord:
                    lexemeValue = "final";
                    break;
                case Lexemes.TypeIntKeyWord:
                    lexemeValue = "int";
                    break;
                case Lexemes.TypeDoubleKeyWord:
                    lexemeValue = "double";
                    break;
                case Lexemes.TypeBooleanKeyWord:
                    lexemeValue = "boolean";
                    break;
                case Lexemes.TypeVoidKeyWord:
                    lexemeValue = "void";
                    break;
                case Lexemes.TypeNewKeyWord:
                    lexemeValue = "new";
                    break;
                case Lexemes.TypeDoubleLiteral:
                    break;
                case Lexemes.TypeIntLiteral:
                    break;
                case Lexemes.TypeBooleanLiteral:
                    break;
                case Lexemes.TypeNullLiteral:
                    lexemeValue = "null";
                    break;
                case Lexemes.TypeIdentifier:
                    lexemeValue = "";
                    break;
                case Lexemes.TypeLessSign:
                    lexemeValue = "<";
                    break;
                case Lexemes.TypeLessOrEqualSign:
                    lexemeValue = "<=";
                    break;
                case Lexemes.TypeMoreSign:
                    lexemeValue = ">";
                    break;
                case Lexemes.TypeMoreOrEqualSign:
                    lexemeValue = ">=";
                    break;
                case Lexemes.TypeEqualSign:
                    lexemeValue = "==";
                    break;
                case Lexemes.TypeNotEqualSign:
                    lexemeValue = "!=";
                    break;
                case Lexemes.TypeAssignmentSign:
                    lexemeValue = "=";
                    break;
                case Lexemes.TypePlus:
                    lexemeValue = "+";
                    break;
                case Lexemes.TypeMinus:
                    lexemeValue = "-";
                    break;
                case Lexemes.TypeIncrement:
                    lexemeValue = "++";
                    break;
                case Lexemes.TypeDecrement:
                    lexemeValue = "--";
                    break;
                case Lexemes.TypeMult:
                    lexemeValue = "*";
                    break;
                case Lexemes.TypeDiv:
                    lexemeValue = "/";
                    break;
                case Lexemes.TypeMod:
                    lexemeValue = "%";
                    break;
                case Lexemes.TypeDot:
                    lexemeValue = ".";
                    break;
                case Lexemes.TypeSemicolon:
                    lexemeValue = ";";
                    break;
                case Lexemes.TypeOpenCurlyBrace:
                    lexemeValue = "{";
                    break;
                case Lexemes.TypeCloseCurlyBrace:
                    lexemeValue = "}";
                    break;
                case Lexemes.TypeOpenParenthesis:
                    lexemeValue = "(";
                    break;
                case Lexemes.TypeCloseParenthesis:
                    lexemeValue = ")";
                    break;
                case Lexemes.TypeComma:
                    lexemeValue = ",";
                    break;
                case Lexemes.TypeError:
                    break;
                case Lexemes.TypeEnd:
                    lexemeValue = "#";
                    break;
                default:
                    break;
            }
            result.Value = lexemeValue;
            return result;
        }

        public Token Clone()
        {
            Token result = new Token(Lexeme, (string)Value.Clone());
            return result;
        }
    }
}
