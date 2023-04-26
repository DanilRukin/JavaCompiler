﻿using JavaCompiler.Common;
using JavaCompiler.LexerAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                    if (token.Lexeme == data.Token.Lexeme) // совпадение с отсканированным терминалом
                    {
                        if (data.Token.Lexeme == Lexemes.TypeEnd)
                            analyze = false;
                        else
                        {
                            token = _lexer.NextToken();
                        }
                    }
                    else
                    {
                        // ошибка
                        if (data.Token.Lexeme == Lexemes.TypeIdentifier)
                            throw new SyntaxErrorException($"Ожидался идентификатор, но отсканирован символ: '{token.Value}'. " +
                                $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                        else
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
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeIdentifier, "") }); // мы не знаем, какой именно идентификатор будет отсканирован
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeClassKeyWord, "class")});
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Ожидался символ: 'class', но отсканирован символ: '{token.Value}'." +
                                    $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //case NonTerminals.Identifier:
                        //    if (token.Lexeme != Lexemes.TypeIdentifier)
                        //    {
                        //        throw new SyntaxErrorException($"Ожидался идентификатор, но отсканирован символ '{token.Value}'." +
                        //            $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                        //    }
                        //    break;
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
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //ClassMemberDeclaration:
                        //    FieldDeclaration
                        //    | MethodDeclaration
                        //    | ClassDeclaration
                        //    | ;
                        case NonTerminals.ClassMemberDeclaration:
                            if (token.Lexeme == Lexemes.TypeClassKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassDeclaration });
                            }
                            else if (token.Lexeme == Lexemes.TypeVoidKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodDeclaration });
                            }
                            else if (token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord
                                || token.Lexeme == Lexemes.TypeFinalKeyWord
                                || token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                Lexer.Position position = _lexer.SavePosition();
                                Token nextToken = _lexer.NextToken();
                                if (nextToken == Token.Default())
                                    throw new SyntaxErrorException("Встречен конец файла");
                                if (nextToken.Lexeme != Lexemes.TypeIdentifier)
                                    throw new SyntaxErrorException($"Ожидался индентификатор, но отсканирован символ: {token.Value}." +
                                        $" Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}.");
                                nextToken = _lexer.NextToken();
                                if (nextToken == Token.Default())
                                    throw new SyntaxErrorException("Встречен конец файла");
                                if (nextToken.Lexeme == Lexemes.TypeOpenParenthesis) // это метод
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodDeclaration });
                                }
                                else // иначе, поле
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.FieldDeclaration });
                                }
                                _lexer.RestorePosition(position);
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //FieldDeclaration:
                        //    Type VariableDeclaratorList;
                        //    | final Type VariableDeclaratorList;
                        case NonTerminals.FieldDeclaration:
                            if (token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord
                                || token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclaratorList });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Type });
                            }
                            else if (token.Lexeme == Lexemes.TypeFinalKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclaratorList });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Type });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeFinalKeyWord, "final") });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //VariableDeclaratorList:
                        //    VariableDeclarator VariableDeclaratorList_1
                        case NonTerminals.VariableDeclaratorList:
                            if (token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclaratorList_1});
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclarator});
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Ожидался индентификатор, но отсканирован символ: {token.Value}." +
                                        $" Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}.");
                            }
                            break;
                        //VariableDeclaratorList_1:
                        //    , VariableDeclarator VariableDeclaratorList_1
                        //    | eps
                        case NonTerminals.VariableDeclaratorList_1:
                            if (token.Lexeme == Lexemes.TypeComma)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclaratorList_1 });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclarator });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeComma, ",") });
                            }
                            else if (token.Lexeme == Lexemes.TypeSemicolon)
                            {
                                Epsilon();
                            }
                            else
                            {
                                throw new SyntaxErrorException("Ожидался символ: ',', но отсканирован символ: '" + token.Value + "'." +
                                    "Строка: " + _lexer.CurrentRow + ", столбец: " + _lexer.CurrentColumn);
                            }
                            break;
                        //VariableDeclarator:
                        //    Identifier
                        //    | Identifier = Expression
                        case NonTerminals.VariableDeclarator:
                            if (token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                Lexer.Position position = _lexer.SavePosition(); // проверяем, содержит ли данное объявление выражение
                                Token nextToken = _lexer.NextToken();
                                if (nextToken.Lexeme == Lexemes.TypeAssignmentSign)  // содержит выражение
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Expression });
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeAssignmentSign, "=") });
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Identifier });
                                }
                                else // не содержит выражение
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Identifier });
                                }
                                _lexer.RestorePosition(position);
                            }
                            break;
                        //MethodDeclaration:
                        //    MethodHeader MethodBody
                        case NonTerminals.MethodDeclaration:
                            if (token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord
                                || token.Lexeme == Lexemes.TypeVoidKeyWord
                                || token.Lexeme == Lexemes.TypeFinalKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodBody });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodHeader });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //MethodHeader:
                        //    ResultType MethodDeclarator
                        //    | final ResultType MethodDeclarator
                        case NonTerminals.MethodHeader:
                            if (token.Lexeme == Lexemes.TypeFinalKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodDeclarator });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ResultType });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeFinalKeyWord, "final") });
                            }
                            else if (token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeVoidKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodDeclarator });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ResultType });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //ResultType:
                        //    Type
                        //    | void
                        case NonTerminals.ResultType:
                            if (token.Lexeme == Lexemes.TypeVoidKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeVoidKeyWord, "void") });
                            }
                            else if (token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord
                                || token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Type });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //MethodDeclarator:
                        //    Identifier()
                        case NonTerminals.MethodDeclarator:
                            if (token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeCloseParenthesis, ")") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeOpenParenthesis, "(") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, Token = new Token(Lexemes.TypeIdentifier, "") });
                            }
                            else
                            {
                                throw new SyntaxErrorException("Ожидалось имя метода, но отсканирован символ: '" + token.Value + "'." +
                                    "Строка: " + _lexer.CurrentRow + ", столбец: " + _lexer.CurrentColumn);
                            }
                            break;
                        //MethodBody:
                        //    Block
                        //    | ;
                        case NonTerminals.MethodBody:
                            if (token.Lexeme == Lexemes.TypeOpenCurlyBrace)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Block });
                            }
                            else if (token.Lexeme == Lexemes.TypeSemicolon)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                            }
                            else
                            {
                                throw new SyntaxErrorException("Ожидался символ '{' или ';', но отсканирован символ: '" + token.Value + "'." +
                                    "Строка: " + _lexer.CurrentRow + ", столбец: " + _lexer.CurrentColumn);
                            }
                            break;
                        //ConstructorDeclaration:
                        //    Identifier() Block
                        case NonTerminals.ConstructorDeclaration:
                            if (token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeCloseParenthesis, ")") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeOpenParenthesis, "(") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, Token = new Token(Lexemes.TypeIdentifier, "") });
                            }
                            else
                            {
                                throw new SyntaxErrorException("Ожидалося конструктор, но отсканирован символ: '" + token.Value + "'." +
                                    "Строка: " + _lexer.CurrentRow + ", столбец: " + _lexer.CurrentColumn);
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
