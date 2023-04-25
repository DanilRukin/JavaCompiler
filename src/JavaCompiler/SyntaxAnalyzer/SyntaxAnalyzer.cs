using JavaCompiler.Common;
using JavaCompiler.LexerAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace JavaCompiler.SyntaxAnalyzer
{
    public class SyntaxAnalyzer
    {
        private const int MAG_SIZE = 5000;
        private Lexer _lexer = new Lexer();
        private Stack<SyntaxData> _mag = new Stack<SyntaxData>(MAG_SIZE);
        private class SyntaxData
        {
            public Token Token { get; set; } = Token.Default();
            public NonTerminals NonTerminal { get; set; } = NonTerminals.Invalid;
            public bool IsTerminal { get; set; } = true;

            public override bool Equals(object? obj)
            {
                if (obj == null) return false;
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                SyntaxData? data = obj as SyntaxData;
                return data.Token.Equals(Token) 
                    && data?.NonTerminal == NonTerminal 
                    && data?.IsTerminal == IsTerminal;
            }

            public static bool operator==(SyntaxData left, SyntaxData right) => left.Equals(right);
            public static bool operator!=(SyntaxData left, SyntaxData right) => !left.Equals(right);
        }

        public void SetText(string text)
        {
            _lexer.SetText(text);
            _mag.Clear();
        }

        public void ClearText()
        {
            _mag.Clear();
            _lexer.ClearText();
        }

        private void Epsilon() => _mag.Pop();

        public void Analyze()
        {
            bool analyze = true;
            Token token;
            _mag.Clear();
            _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.CompilationUnit });  // вносим аксиому в начало
            SyntaxData data;
            token = _lexer.NextToken();
            while (analyze)
            {
                data = _mag.Pop();
                if (data.IsTerminal)  // в верхушке магазина терминал
                {
                    if (token.Equals(data.Token)) // совпадение с отсканированным терминалом
                    {
                        if (data.Token.Equals(Token.Default()))
                            analyze = false;
                        else
                        {
                            token = _lexer.NextToken();
                        }
                    }
                    else
                    {
                        // ошибка
                        throw new SyntaxErrorException($"Ожидался символ: '{data.Token.Value}', но отсканирован символ: '{token.Value}'. " +
                            $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                    }
                }
                else
                {
                    switch(data.NonTerminal)
                    {
                        //CompilationUnit:
                        //    | TypeDeclarations
                        //    | ε
                        case NonTerminals.CompilationUnit:
                            if (token.Equals(Token.Default()))
                                Epsilon();
                            else
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.TypeDeclarations });
                            }
                            break;
                        //TypeDeclarations:
                        //    ClassDeclaration
                        //    | ClassDeclaration TypeDeclarations
                        case NonTerminals.TypeDeclarations:
                            if (token.Lexeme == Lexemes.TypeClassKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.TypeDeclarations });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassDeclaration });
                            }
                            else
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassDeclaration });
                            }
                            break;
                        //ClassDeclaration:
                        //    class Identifier ClassBody
                        case NonTerminals.ClassDeclaration:
                            if (token.Lexeme == Lexemes.TypeClassKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassBody });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Identifier });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeClassKeyWord, "class")});
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Ожидался символ: 'class', но отсканирован символ: '{token.Value}'." +
                                    $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        case NonTerminals.Identifier:
                            if (token.Lexeme != Lexemes.TypeIdentifier)
                            {
                                throw new SyntaxErrorException($"Ожидался идентификатор, но отсканирован символ '{token.Value}'." +
                                    $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //ClassBody:
                        //    { ClassBodyDeclaration}
                        case NonTerminals.ClassBody:
                            if (token.Lexeme == Lexemes.TypeOpenCurlyBrace)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeCloseCurlyBrace, "}") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassBodyDeclaration });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeOpenCurlyBrace, "{") });
                            }
                            else
                            {
                                throw new SyntaxErrorException("Ожидался символ: '{', но отсканирован символ: '" + token.Value + "'." +
                                    "Строка: " + _lexer.CurrentRow + ", столбец: " + _lexer.CurrentColumn);
                            }
                            break;
                        //ClassBodyDeclaration:
                        //    ClassMemberDeclaration
                        //    | ConstructorDeclaration
                        //    | Block
                        //    | ε
                        case NonTerminals.ClassBodyDeclaration:
                            if (token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeVoidKeyWord
                                || token.Lexeme == Lexemes.TypeFinalKeyWord
                                || token.Lexeme == Lexemes.TypeClassKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassMemberDeclaration });
                            }
                            else if (token.Lexeme == Lexemes.TypeOpenCurlyBrace)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Block });
                            }
                            else if (token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                // либо объявление конструктора, либо описание членов класса
                                // TODO: внедрить определение конструктора (это уже семантика)
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassMemberDeclaration });
                            }
                            else if (token.Lexeme == Lexemes.TypeCloseCurlyBrace)
                            {
                                Epsilon();
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Lexeme}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        default:
                            throw new SyntaxErrorException($"Неопределенная ошибка при анализе нетерминала...({data.NonTerminal})");
                    }
                }
            }
        }
    }
}
