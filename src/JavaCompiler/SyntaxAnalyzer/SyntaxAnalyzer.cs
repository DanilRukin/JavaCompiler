using JavaCompiler.Common;
using JavaCompiler.LexerAnalyzer;
using System;
using System.Collections.Generic;
using System.IO;
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
                        //Block:
                        //    { BlockStatements}
                        //    | { }
                        case NonTerminals.Block:
                            if (token.Lexeme == Lexemes.TypeOpenCurlyBrace)
                            {
                                Lexer.Position position = _lexer.SavePosition();
                                Token nextToken = _lexer.NextToken();
                                if (nextToken.Lexeme == Lexemes.TypeCloseCurlyBrace) // пустой блок
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeCloseCurlyBrace, "}") });
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeOpenCurlyBrace, "{") });
                                }
                                else // внутри блока есть выражения
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeCloseCurlyBrace, "}") });
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.BlockStatements });
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeOpenCurlyBrace, "{") });
                                }
                                _lexer.RestorePosition(position);
                            }
                            else
                            {
                                throw new SyntaxErrorException("Ожидался символ '{' или ';', но отсканирован символ: '" + token.Value + "'." +
                                    "Строка: " + _lexer.CurrentRow + ", столбец: " + _lexer.CurrentColumn);
                            }
                            break;
                        //BlockStatements:
                        //    BlockStatement
                        //    | BlockStatement BlockStatements
                        case NonTerminals.BlockStatements:
                            if (token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord
                                || token.Lexeme == Lexemes.TypeVoidKeyWord
                                || token.Lexeme == Lexemes.TypeFinalKeyWord
                                || token.Lexeme == Lexemes.TypeClassKeyWord
                                || token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.BlockStatements });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.BlockStatement });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //BlockStatement:
                        //    LocalVariableDeclaration;
                        //    | ClassDeclaration
                        //    | MethodDeclaration
                        //    | Statement
                        //    | eps
                        case NonTerminals.BlockStatement:
                            if (token.Lexeme == Lexemes.TypeClassKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassDeclaration });
                            }
                            else if (token.Lexeme == Lexemes.TypeCloseCurlyBrace)
                            {
                                Epsilon();
                            }
                            else if (token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord)
                            {
                                // либо переменные, либо методы
                                Lexer.Position position = _lexer.SavePosition();
                                Token nextToken = _lexer.NextToken();
                                if (nextToken.Lexeme == Lexemes.TypeEnd)
                                {
                                    throw new SyntaxErrorException("Встречен конец файла");
                                }
                                if (nextToken.Lexeme == Lexemes.TypeIdentifier)
                                {
                                    nextToken = _lexer.NextToken();
                                    if (nextToken.Lexeme == Lexemes.TypeEnd)
                                    {
                                        throw new SyntaxErrorException("Встречен конец файла");
                                    }
                                    if (nextToken.Lexeme == Lexemes.TypeOpenParenthesis) // метод
                                    {
                                        _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodDeclaration });
                                    }
                                    else // локальные переменные
                                    {
                                        _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                                        _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.LocalVariableDeclaration });
                                    }
                                }
                                else
                                {
                                    throw new SyntaxErrorException($"Ожидался индентификатор, но отсканирован символ: {token.Value}." +
                                        $" Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}.");
                                }
                                _lexer.RestorePosition(position);
                            }
                            else // что делаем со Statement?
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Statement });
                            }
                            break;
                        //LocalVariableDeclaration:
                        //    Type VariableDeclaratorList
                        //    | final Type VariableDeclaratorList
                        case NonTerminals.LocalVariableDeclaration:
                            if (token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord
                                || token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclaratorList });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Type });
                            }
                            else if (token.Lexeme == Lexemes.TypeFinalKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclaratorList });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Type });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeFinalKeyWord, "final") });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //Statement:
                        //    WhileStatement
                        //    | Block
                        //    | ;
                        //    | ExpressionStatement
                        //    | ReturnStatement
                        case NonTerminals.Statement:
                            if (token.Lexeme == Lexemes.TypeWhileKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.WhileStatement });
                            }
                            else if (token.Lexeme == Lexemes.TypeReturnKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ReturnStatement });
                            }
                            else if (token.Lexeme == Lexemes.TypeSemicolon)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                            }
                            else if (token.Lexeme == Lexemes.TypeOpenCurlyBrace)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Block });
                            }
                            else if (token.Lexeme == Lexemes.TypeIncrement
                                || token.Lexeme == Lexemes.TypeDecrement
                                || token.Lexeme == Lexemes.TypeIdentifier
                                || token.Lexeme == Lexemes.TypeNewKeyWord
                                || token.Lexeme == Lexemes.TypeOpenParenthesis
                                || token.Lexeme == Lexemes.TypeMult
                                || token.Lexeme == Lexemes.TypeDiv
                                || token.Lexeme == Lexemes.TypeMod
                                || token.Lexeme == Lexemes.TypeLessSign
                                || token.Lexeme == Lexemes.TypeLessOrEqualSign
                                || token.Lexeme == Lexemes.TypeMoreSign
                                || token.Lexeme == Lexemes.TypeMoreOrEqualSign
                                || token.Lexeme == Lexemes.TypeEqualSign
                                || token.Lexeme == Lexemes.TypeNotEqualSign)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ExpressionStatement });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //ExpressionStatement:
                        //    StatementExpression;
                        case NonTerminals.ExpressionStatement:
                            if (token.Lexeme == Lexemes.TypeIncrement
                                || token.Lexeme == Lexemes.TypeDecrement
                                || token.Lexeme == Lexemes.TypeIdentifier
                                || token.Lexeme == Lexemes.TypeNewKeyWord
                                || token.Lexeme == Lexemes.TypeOpenParenthesis)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.StatementExpression });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //StatementExpression:
                        //    Assignment
                        //    | PreIncrementExpression
                        //    | PreDecrementExpression
                        //    | PostIncrementExpression
                        //    | PostDecrementExpression
                        //    | MethodInvocation
                        //    | ClassInstanceCreationExpression
                        case NonTerminals.StatementExpression:
                            if (token.Lexeme == Lexemes.TypeNewKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassInstanceCreationExpression });
                            }
                            else if (token.Lexeme == Lexemes.TypeIncrement)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.PreIncrementExpression });
                            }
                            else if (token.Lexeme == Lexemes.TypeDecrement)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.PreDecrementExpression });
                            }
                            else if (token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                // TODO: придумать, что делать с PostIncrementExpression, PostDecrementExpression, MethodInvocation, Assignment
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //WhileStatement:
                        //    while (Expression) Statement
                        case NonTerminals.WhileStatement:
                            if (token.Lexeme == Lexemes.TypeWhileKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Statement });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeCloseParenthesis, ")") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Expression });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeOpenParenthesis, "(") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeWhileKeyWord, "while") });
                            }
                            else
                            {
                                throw new SyntaxErrorException("Ожидался символ 'while', но отсканирован символ: '" + token.Value + "'." +
                                    "Строка: " + _lexer.CurrentRow + ", столбец: " + _lexer.CurrentColumn);
                            }
                            break;
                        //ReturnStatement:
                        //    return;
                        //    | return Expression;
                        case NonTerminals.ReturnStatement:
                            if (token.Lexeme == Lexemes.TypeReturnKeyWord)
                            {
                                Lexer.Position position = _lexer.SavePosition();
                                Token nextToken = _lexer.NextToken();
                                if (nextToken.Lexeme == Lexemes.TypeEnd)
                                {
                                    throw new SyntaxErrorException("Встречен конец файла");
                                }
                                if (nextToken.Lexeme == Lexemes.TypeSemicolon)
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeReturnKeyWord, "return") });
                                }
                                else
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Expression });
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeReturnKeyWord, "return") });
                                }
                                _lexer.RestorePosition(position);
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Ожидался символ: 'return', но отсканирован символ: '{token.Value}'." +
                                    $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //ClassInstanceCreationExpression:
                        //    | UnqualifiedClassInstanceCreationExpression ClassInstanceCreationExpression_1
                        //    | ExpressionName ClassInstanceCreationExpression_1
                        //    | Literal ClassInstanceCreationExpression_1
                        //    | TypeName ClassInstanceCreationExpression_1
                        //    | FieldAccess ClassInstanceCreationExpression_1
                        //    | MethodInvocation ClassInstanceCreationExpression_1
                        case NonTerminals.ClassInstanceCreationExpression:
                            if (token.Lexeme == Lexemes.TypeNewKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassInstanceCreationExpression_1 });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.UnqualifiedClassInstanceCreationExpression });
                            }
                            else if (token.Lexeme == Lexemes.TypeBooleanLiteral
                                || token.Lexeme == Lexemes.TypeDoubleLiteral
                                || token.Lexeme == Lexemes.TypeIntLiteral)
                            {
                                _mag.Push(new SyntaxData { IsTerminal = false, NonTerminal = NonTerminals.ClassInstanceCreationExpression_1 });
                                _mag.Push(new SyntaxData { IsTerminal = false, NonTerminal = NonTerminals.Literal });
                            }
                            else if (token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                // TODO: ExpressionName выкинуть
                                Lexer.Position position = _lexer.SavePosition();
                                Token nextToken = _lexer.NextToken();
                                if (nextToken == Token.Default())
                                    throw new SyntaxErrorException($"Встречен конец файла. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                                if (nextToken.Lexeme == Lexemes.TypeOpenParenthesis) // вызов метода
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassInstanceCreationExpression_1 });
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodInvocation });
                                }
                                else // либо имя TypeName, либо FieldAccees, либо ExpressionName - это уже семантика
                                {
                                    // TODO: на уровне семантики определить что это такое, пока что буду использовать FieldAccees
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassInstanceCreationExpression_1 });
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.FieldAccess });
                                }
                                _lexer.RestorePosition(position);
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Ожидался символ: 'new' или идентификатор, но отсканирован символ: '{token.Value}'." +
                                    $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        // ClassInstanceCreationExpression_1:
                        //  | .UnqualifiedClassInstanceCreationExpression ClassInstanceCreationExpression_1
                        //  | eps
                        case NonTerminals.ClassInstanceCreationExpression_1:
                            if (token.Lexeme == Lexemes.TypeDot)
                            {
                                _mag.Push(new SyntaxData { IsTerminal = false, NonTerminal = NonTerminals.ClassInstanceCreationExpression_1 });
                                _mag.Push(new SyntaxData { IsTerminal = false, NonTerminal = NonTerminals.UnqualifiedClassInstanceCreationExpression });
                                _mag.Push(new SyntaxData { IsTerminal = true, Token = new Token(Lexemes.TypeDot, ".") });
                            }
                            else if (token.Lexeme == Lexemes.TypeSemicolon)
                            {
                                Epsilon();
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Ожидался символ: '.' или ';', но отсканирован символ: '{token.Value}'." +
                                    $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //UnqualifiedClassInstanceCreationExpression:
                        //    new Identifier()
                        case NonTerminals.UnqualifiedClassInstanceCreationExpression:
                            if (token.Lexeme == Lexemes.TypeNewKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeCloseParenthesis, ")") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeOpenParenthesis, "(") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeIdentifier, "") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeNewKeyWord, "new") });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Ожидался символ: 'new', но отсканирован символ: '{token.Value}'." +
                                    $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //FieldAccess:
                        //  Literal FieldAccess_1
                        //  | TypeName FieldAccess_1
                        //  | ClassInstanceCreationExpression FieldAccess_1
                        //  | MethodInvocation FieldAccess_1
                        case NonTerminals.FieldAccess:
                            if (token.Lexeme == Lexemes.TypeIdentifier
                                || token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeIdentifier, "") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeDot, ".") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Primary });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        //Primary:
                        //    Literal
                        //    | TypeName
                        //    | FieldAccess
                        //    | ClassInstanceCreationExpression
                        //    | MethodInvocation
                        case NonTerminals.Primary:
                            if (token.Lexeme == Lexemes.TypeNullLiteral
                                || token.Lexeme == Lexemes.TypeBooleanLiteral)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Literal });
                            }
                            else if (token.Lexeme == Lexemes.TypeNewKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassInstanceCreationExpression });
                            }
                            else
                            {
                                // TODO: устранить левую рекурсию, связанную с Primary (FieldAccess)
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
