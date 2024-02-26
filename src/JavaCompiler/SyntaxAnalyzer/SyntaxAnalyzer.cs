using JavaCompiler.Common;
using JavaCompiler.LexerAnalyzer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Security.Principal;
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
                        // TypeDeclarations: ClassDeclaration | ClassDeclaration TypeDeclarations | EPSILON
                        case NonTerminals.TypeDeclarations:
                            if (token.Lexeme == Lexemes.TypeClassKeyWord
                                || token.Lexeme == Lexemes.TypeFinalKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.TypeDeclarations });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassDeclaration });
                            }
                            else if (token.Lexeme == Lexemes.TypeEnd)
                            {
                                Epsilon();
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Ожидалось определение класса, но отсканирован символ: '{token.Value}'." +
                                    $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        // ClassDeclaration: final class Identifier ClassBody | class Identifier ClassBody
                        case NonTerminals.ClassDeclaration:
                            if (token.Lexeme == Lexemes.TypeClassKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassBody });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeIdentifier, "") }); // мы не знаем, какой именно идентификатор будет отсканирован
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeClassKeyWord, "class")});
                            }
                            else if (token.Lexeme == Lexemes.TypeFinalKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassBody });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeIdentifier, "") }); // мы не знаем, какой именно идентификатор будет отсканирован
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeClassKeyWord, "class") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeClassKeyWord, "final") });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Ожидался символ: 'class', но отсканирован символ: '{token.Value}'." +
                                    $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        // ClassBody: { ClassBodyDeclarations }
                        case NonTerminals.ClassBody:
                            if (token.Lexeme == Lexemes.TypeOpenCurlyBrace)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeCloseCurlyBrace, "}") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassBodyDeclarations });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeOpenCurlyBrace, "{") });
                            }
                            else
                            {
                                throw new SyntaxErrorException("Ожидался символ: '{', но отсканирован символ: '" + token.Value + "'." +
                                    "Строка: " + _lexer.CurrentRow + ", столбец: " + _lexer.CurrentColumn);
                            }
                            break;
                        // ClassBodyDeclarations: ClassBodyDeclaration | ClassBodyDeclaration ClassBodyDeclarations | EPSILON
                        case NonTerminals.ClassBodyDeclarations:
                            if (token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeVoidKeyWord
                                || token.Lexeme == Lexemes.TypeFinalKeyWord
                                || token.Lexeme == Lexemes.TypeClassKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord
                                || token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassBodyDeclarations});
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassBodyDeclaration });
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
                        // ClassBodyDeclaration: ClassMemberDeclaration | ConstructorDeclaration
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
                            else if (token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                Lexer.Position position = _lexer.SavePosition();
                                Token nextToken = _lexer.NextToken();
                                if (nextToken == Token.Default())
                                    throw new SyntaxErrorException("Встречен конец файла");
                                if (nextToken.Lexeme == Lexemes.TypeOpenParenthesis) // встречен конструктор, только надо, чтобы этот идентификтор совпадал с именем класса
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ConstructorDeclaration });
                                }
                                else if (nextToken.Lexeme == Lexemes.TypeIdentifier) // иначе, метод
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodDeclaration });
                                }
                                else
                                {
                                    throw new SyntaxErrorException($"Ожидался метод или конструктор, но отсканирован символ: '{token.Value}'" +
                                        $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                                }
                                _lexer.RestorePosition(position);                              
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Ожидался член класса или конструктор, но отсканирован символ: '{token.Value}'" +
                                    $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        // ClassMemberDeclaration: FieldDeclaration | MethodDeclaration | ClassDeclaration
                        case NonTerminals.ClassMemberDeclaration:
                            if (token.Lexeme == Lexemes.TypeClassKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassDeclaration });
                            }
                            else if (token.Lexeme == Lexemes.TypeVoidKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodDeclaration });
                            }
                            // либо метод, либо поле
                            else if (token.Lexeme == Lexemes.TypeIntKeyWord  
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord)
                            {
                                Lexer.Position position = _lexer.SavePosition();
                                Token nextToken = _lexer.NextToken();
                                if (nextToken == Token.Default())
                                    throw new SyntaxErrorException("Встречен конец файла");
                                if (nextToken.Lexeme != Lexemes.TypeIdentifier)
                                    throw new SyntaxErrorException($"Ожидался индентификатор, но отсканирован символ: {token.Value}." +
                                        $" Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}.");
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = ResolveMethodOrFieldDeclaration() });
                                _lexer.RestorePosition(position);
                            }
                            else if (token.Lexeme == Lexemes.TypeIdentifier) // это может быть сложный идентификтор
                            {
                                Lexer.Position position = _lexer.SavePosition();
                                Token nextToken = _lexer.NextToken();
                                if (nextToken == Token.Default())
                                    throw new SyntaxErrorException("Встречен конец файла");
                                if (nextToken.Lexeme == Lexemes.TypeDot) // это составной идентификатор, означающий тип поля или тип возвращаемого функцией результата
                                {
                                    // надо 'пропустить' составной идентификтор
                                    while (nextToken.Lexeme == Lexemes.TypeDot)
                                    {
                                        nextToken = _lexer.NextToken();
                                        if (nextToken == Token.Default())
                                            throw new SyntaxErrorException("Встречен конец файла");
                                        if (nextToken.Lexeme != Lexemes.TypeIdentifier)
                                            throw new SyntaxErrorException($"Ожидался идентификтор в составном имени типа, но отсканирован символ: '{token.Value}'." +
                                                $" Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}.");
                                        nextToken = _lexer.NextToken();
                                    }
                                    // составной идентификатор типа закончился, получена следующая после пробела лексема
                                    if (nextToken.Lexeme == Lexemes.TypeIdentifier) // наткнулись на имя метода или поля
                                    {
                                        _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = ResolveMethodOrFieldDeclaration() });
                                    }
                                    else
                                        throw new SyntaxErrorException($"Ожидалось имя метода или поля, но отсканирован символ: '{token.Value}'." +
                                            $" Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}.");
                                }
                                else if (nextToken.Lexeme == Lexemes.TypeIdentifier) // наткнулись на имя метода или поля
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = ResolveMethodOrFieldDeclaration() });
                                }
                                _lexer.RestorePosition(position);
                            }
                            else if (token.Lexeme == Lexemes.TypeFinalKeyWord) // это может быть либо класс, либо метод, либо просто поле, короче, тот же самый ClassMemberDeclaration, только с final
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassMemberDeclaration });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeFinalKeyWord, "final") });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        // FieldDeclaration: final Type VariableDeclarators ; | Type VariableDeclarators ;
                        case NonTerminals.FieldDeclaration:
                            if (token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord
                                || token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclarators });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Type });
                            }
                            else if (token.Lexeme == Lexemes.TypeFinalKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclarators });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Type });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeFinalKeyWord, "final") });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        // VariableDeclarators: VariableDeclarator | VariableDeclarator , VariableDeclarators
                        case NonTerminals.VariableDeclarators: // либо просто имя, либо список имен через запятую
                            if (token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                Lexer.Position position = _lexer.SavePosition();
                                Token nextToken = _lexer.NextToken();
                                if (nextToken == Token.Default())
                                    throw new SyntaxErrorException("Встречен конец файла");
                                if (nextToken.Lexeme == Lexemes.TypeComma)
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclarators });
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeComma, ",") });
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclarator });
                                }
                                else
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclarator });
                                }
                                _lexer.RestorePosition(position);                                
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Ожидался индентификатор, но отсканирован символ: {token.Value}." +
                                        $" Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}.");
                            }
                            break;
                        // VariableDeclarator: Identifier | Identifier = Expression
                        case NonTerminals.VariableDeclarator:
                            if (token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                Lexer.Position position = _lexer.SavePosition(); // проверяем, содержит ли данное объявление выражение
                                Token nextToken = _lexer.NextToken();
                                if (nextToken == Token.Default())
                                    throw new SyntaxErrorException("Встречен конец файла");
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
                        // MethodDeclaration: MethodHeader MethodBody
                        case NonTerminals.MethodDeclaration:
                            if (token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord
                                || token.Lexeme == Lexemes.TypeVoidKeyWord
                                || token.Lexeme == Lexemes.TypeFinalKeyWord
                                || token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodBody });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodHeader });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                            // MethodHeader:
                            //      final Type Identifier()
                            //      | final void Identifier()
                            //      | Type Identifier()
                            //      | void Identifier()
                        case NonTerminals.MethodHeader:
                            if (token.Lexeme == Lexemes.TypeFinalKeyWord)
                            {
                                Lexer.Position position = _lexer.SavePosition();
                                Token nextToken = _lexer.NextToken();
                                if (nextToken == Token.Default())
                                    throw new SyntaxErrorException("Встречен конец файла");
                                if (token.Lexeme == Lexemes.TypeVoidKeyWord)
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeCloseParenthesis, ")") });
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeOpenParenthesis, "(") });
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Identifier });
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeVoidKeyWord, "void") });
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeFinalKeyWord, "final") });
                                }
                                else
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeCloseParenthesis, ")") });
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeOpenParenthesis, "(") });
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Identifier });
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Type });
                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeFinalKeyWord, "final") });
                                }
                                _lexer.RestorePosition(position);
                                
                            }
                            else if (token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeIdentifier
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeCloseParenthesis, ")") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeOpenParenthesis, "(") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Identifier });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Type });
                            }
                            else if (token.Lexeme == Lexemes.TypeVoidKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeCloseParenthesis, ")") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeOpenParenthesis, "(") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Identifier });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeVoidKeyWord, "void") });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        // MethodBody: Block | ;
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
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        // ConstructorDeclaration:
                        //      final Identifier() Block
                        //      | Identifier() Block
                        case NonTerminals.ConstructorDeclaration:
                            if (token.Lexeme == Lexemes.TypeFinalKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Block });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeCloseParenthesis, ")") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeOpenParenthesis, "(") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, Token = new Token(Lexemes.TypeIdentifier, "") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeFinalKeyWord, "final") });
                            }
                            else if (token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Block });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeCloseParenthesis, ")") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeOpenParenthesis, "(") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, Token = new Token(Lexemes.TypeIdentifier, "") });
                            }
                            else
                            {
                                throw new SyntaxErrorException("Ожидался конструктор, но отсканирован символ: '" + token.Value + "'." +
                                    "Строка: " + _lexer.CurrentRow + ", столбец: " + _lexer.CurrentColumn);
                            }
                            break;
                        // Block:
                        //    { BlockStatements }
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
                        // BlockStatements: BlockStatement | BlockStatement BlockStatements | EPSILON
                        case NonTerminals.BlockStatements:
                            if (token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord
                                || token.Lexeme == Lexemes.TypeVoidKeyWord
                                || token.Lexeme == Lexemes.TypeFinalKeyWord
                                || token.Lexeme == Lexemes.TypeClassKeyWord
                                || token.Lexeme == Lexemes.TypeIdentifier
                                || token.Lexeme == Lexemes.TypeReturnKeyWord
                                || token.Lexeme == Lexemes.TypeSemicolon
                                || token.Lexeme == Lexemes.TypeWhileKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.BlockStatements });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.BlockStatement });
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
                        // BlockStatement: Type VariableDeclarators ; | Statement
                        case NonTerminals.BlockStatement:
                            bool notDefined = true;
                            if (token.Lexeme == Lexemes.TypeClassKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ClassDeclaration });
                                notDefined = false;
                            }
                            else if (token.Lexeme == Lexemes.TypeFinalKeyWord) // MethodDeclaration, ClassDeclaration также может начинаться с final
                            {
                                Lexer.Position position = _lexer.SavePosition();
                                Token nextToken = _lexer.NextToken();
                                if (nextToken.Lexeme == Lexemes.TypeEnd)
                                {
                                    throw new SyntaxErrorException("Встречен конец файла");
                                }
                                if (nextToken.Lexeme == Lexemes.TypeVoidKeyWord) // это MethodDeclaration
                                {
                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodDeclaration });
                                    notDefined = false;
                                }
                                else if (nextToken.Lexeme == Lexemes.TypeIntKeyWord // либо MethodDeclaration, либо Type VariableDeclarators;
                                    || nextToken.Lexeme == Lexemes.TypeDoubleKeyWord
                                    || nextToken.Lexeme == Lexemes.TypeBooleanKeyWord
                                    || nextToken.Lexeme == Lexemes.TypeIdentifier)
                                {
                                    if (nextToken.Lexeme == Lexemes.TypeIdentifier) // может быть, например, так Idenifier.Identifier
                                    {
                                        nextToken = _lexer.NextToken();
                                        if (nextToken.Lexeme == Lexemes.TypeEnd)
                                        {
                                            throw new SyntaxErrorException("Встречен конец файла");
                                        }
                                        if (nextToken.Lexeme == Lexemes.TypeOpenParenthesis) // конструктор
                                        {
                                            _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.ConstructorDeclaration });
                                            _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeFinalKeyWord, "final") });
                                            notDefined = false;
                                        }
                                        if (nextToken.Lexeme == Lexemes.TypeDot) // Составной (сложный) тип
                                        {
                                            while (nextToken.Lexeme == Lexemes.TypeDot)
                                            {
                                                nextToken = _lexer.NextToken();
                                                if (nextToken.Lexeme != Lexemes.TypeIdentifier) // после '.' ожидаем только Identifier
                                                {
                                                    throw new SyntaxErrorException($"Ожидался идентификатор, но отсканирован символ: {nextToken.Value}. " +
                                                        $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}.");
                                                }
                                                nextToken = _lexer.NextToken();
                                            }
                                            // Встречено что-то, кроме '.', а значит, можно заносить в магазин инфу о дальнейшем содержимом
                                            if (nextToken.Lexeme == Lexemes.TypeIdentifier) // Либо метод, либо переменные
                                            {
                                                nextToken = _lexer.NextToken();
                                                if (nextToken.Lexeme == Lexemes.TypeEnd)
                                                {
                                                    throw new SyntaxErrorException("Встречен конец файла");
                                                }
                                                if (nextToken.Lexeme == Lexemes.TypeOpenParenthesis) // это MethodDeclaration
                                                {
                                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodDeclaration });
                                                    notDefined = false;
                                                }
                                                else // final Type VariableDeclarators;
                                                {
                                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclarators });
                                                    _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Type });
                                                    _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeFinalKeyWord, "final") });
                                                    notDefined = false;
                                                }
                                            }
                                            else
                                            {
                                                throw new SyntaxErrorException($"Ожидался идентификатор, либо '.', но отсканирован символ: {nextToken.Value}. " +
                                                    $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}.");
                                            }
                                        }
                                        else if (nextToken.Lexeme == Lexemes.TypeIdentifier) // Либо метод, либо переменные
                                        {
                                            nextToken = _lexer.NextToken();
                                            if (nextToken.Lexeme == Lexemes.TypeEnd)
                                            {
                                                throw new SyntaxErrorException("Встречен конец файла");
                                            }
                                            if (nextToken.Lexeme == Lexemes.TypeOpenParenthesis) // это MethodDeclaration
                                            {
                                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodDeclaration });
                                                notDefined = false;
                                            }
                                            else // final Type VariableDeclaratorList;
                                            {
                                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclarators });
                                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Type });
                                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeFinalKeyWord, "final") });
                                                notDefined = false;
                                            }
                                        }
                                        else
                                        {
                                            throw new SyntaxErrorException($"Ожидался идентификатор, либо '.', но отсканирован символ: {nextToken.Value}. " +
                                                $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}.");
                                        }
                                    }
                                    else // int, double, boolean... следующим должен быть Identifier
                                    {
                                        nextToken = _lexer.NextToken();
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
                                            if (nextToken.Lexeme == Lexemes.TypeOpenParenthesis) // это функция, а значит, это MethodDeclaration
                                            {
                                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodDeclaration });
                                                notDefined = false;
                                            }
                                            else // final Type VariableDeclarators;
                                            {
                                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclarators });
                                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Type });
                                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeFinalKeyWord, "final") });
                                                notDefined = false;
                                            }
                                        }
                                        else
                                        {
                                            throw new SyntaxErrorException($"Ожидался идентификатор, но отсканирован символ: {nextToken.Value}. " +
                                                $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}.");
                                        }
                                    }
                                }
                                else
                                {
                                    throw new SyntaxErrorException($"Ожидался тип, но отсканирован символ: {nextToken.Value}. " +
                                        $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}.");
                                }
                                _lexer.RestorePosition(position);
                            }
                            else if (token.Lexeme == Lexemes.TypeOpenCurlyBrace
                                || token.Lexeme == Lexemes.TypeWhileKeyWord
                                || token.Lexeme == Lexemes.TypeReturnKeyWord
                                || token.Lexeme == Lexemes.TypeSemicolon
                                || token.Lexeme == Lexemes.TypeNewKeyWord
                                || token.Lexeme == Lexemes.TypeIdentifier) // Statement
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Statement });
                                notDefined = false;
                            }
                            else if (token.Lexeme == Lexemes.TypeCloseCurlyBrace)
                            {
                                Epsilon();
                                notDefined = false;
                            }
                            else if (token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord
                                || token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                // либо переменные, либо методы
                                Lexer.Position position = _lexer.SavePosition();
                                if (token.Lexeme == Lexemes.TypeIdentifier) // Возможно, составной тип Identifier.Identifier
                                {
                                    Token nextToken = _lexer.NextToken();
                                    if (nextToken.Lexeme == Lexemes.TypeEnd)
                                    {
                                        throw new SyntaxErrorException("Встречен конец файла");
                                    }
                                    if (nextToken.Lexeme == Lexemes.TypeDot) // Составной тип
                                    {
                                        while (nextToken.Lexeme == Lexemes.TypeDot)
                                        {
                                            nextToken = _lexer.NextToken();
                                            if (nextToken.Lexeme != Lexemes.TypeIdentifier) // после '.' ожидаем только Identifier
                                            {
                                                throw new SyntaxErrorException($"Ожидался идентификатор, но отсканирован символ: {nextToken.Value}. " +
                                                    $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}.");
                                            }
                                            nextToken = _lexer.NextToken();
                                        }
                                        // Встречено что-то, кроме '.', а значит, можно заносить в магазин инфу о дальнейшем содержимом
                                        if (nextToken.Lexeme == Lexemes.TypeIdentifier) // Либо метод, либо переменные
                                        {
                                            nextToken = _lexer.NextToken();
                                            if (nextToken.Lexeme == Lexemes.TypeEnd)
                                            {
                                                throw new SyntaxErrorException("Встречен конец файла");
                                            }
                                            if (nextToken.Lexeme == Lexemes.TypeOpenParenthesis) // это MethodDeclaration
                                            {
                                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodDeclaration });
                                                notDefined = false;
                                            }
                                            else // Type VariableDeclaratorList;
                                            {
                                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclarators });
                                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Type });
                                                notDefined = false;
                                            }
                                        }
                                    }
                                    else // Просто тип
                                    {
                                        nextToken = _lexer.NextToken();
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
                                            if (nextToken.Lexeme == Lexemes.TypeOpenParenthesis) // MethodDeclaration
                                            {
                                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.MethodDeclaration });
                                                notDefined = false;
                                            }
                                            else
                                            {
                                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclarators });
                                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Type });
                                                notDefined = false;
                                            }
                                        }
                                        else
                                        {
                                            throw new SyntaxErrorException($"Ожидался индентификатор, но отсканирован символ: {token.Value}." +
                                                $" Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}.");
                                        }
                                    }
                                }
                                else
                                {
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
                                            notDefined = false;
                                        }
                                        else // локальные переменные
                                        {
                                            _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                                            _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.VariableDeclarators });
                                            _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Type });
                                            notDefined = false;
                                        }
                                    }
                                    else
                                    {
                                        throw new SyntaxErrorException($"Ожидался индентификатор, но отсканирован символ: {token.Value}." +
                                            $" Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}.");
                                    }
                                }
                                _lexer.RestorePosition(position);
                            }
                            if (notDefined) // если ничего выше не отработало, то ожидаю здесь Statement
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Statement });
                            }
                            break;
                        //Statement:
                        //    WhileStatement
                        //    | Block
                        //    | ;
                        //    | StatementExpression;
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
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeSemicolon, ";") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.StatementExpression });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;
                        // LeftHandSide: Name | FieldAccess
                        case NonTerminals.LeftHandSide:
                            if (token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                // на семантическом уровне решим имя это или обращение к полю
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.FieldAccess });
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Ожидался индентификатор, но отсканирован символ: {token.Value}." +
                                            $" Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}.");
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
                        // AssignmentOperator: = | *= | /= | %= | += | -=
                        case NonTerminals.AssignmentOperator:
                            if (token.Lexeme == Lexemes.TypeAssignmentSign)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeAssignmentSign, "=") });
                            }
                            else if (token.Lexeme == Lexemes.TypeMult)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeAssignmentSign, "=") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeMult, "*") });
                            }
                            else if (token.Lexeme == Lexemes.TypeDiv)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeAssignmentSign, "=") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeDiv, "/") });
                            }
                            else if (token.Lexeme == Lexemes.TypeMod)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeAssignmentSign, "=") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeMod, "%") });
                            }
                            else if (token.Lexeme == Lexemes.TypePlus)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeAssignmentSign, "=") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypePlus, "+") });
                            }
                            else if (token.Lexeme == Lexemes.TypeMinus)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeAssignmentSign, "=") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeMinus, "-") });
                            }
                            break;

                           
                        // FieldAccess:
                        //      PrimaryExpression.Identifier | Identifier
                        case NonTerminals.FieldAccess:
                            if (token.Lexeme == Lexemes.TypeIntKeyWord
                                || token.Lexeme == Lexemes.TypeDoubleKeyWord
                                || token.Lexeme == Lexemes.TypeBooleanKeyWord)
                            {
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.FieldAccess_1 });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeIdentifier, "") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeDot, ".") });
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.Literal });
                            }
                            else if (token.Lexeme == Lexemes.TypeIdentifier)
                            {
                                Lexer.Position position = _lexer.SavePosition();
                                Token nextToken = _lexer.NextToken();
                                
                                _lexer.RestorePosition(position);
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Неверный символ '{token.Value}'. Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
                            }
                            break;

                        //  FieldAccess_1:
                        //      | .Identifier FieldAccess_1
                        //      | eps
                        case NonTerminals.FieldAccess_1:
                            if (token.Lexeme == Lexemes.TypeDot)
                            {
                                // проверяем на eps
                                Lexer.Position position = _lexer.SavePosition();
                                Token nextToken = _lexer.NextToken();
                                if (nextToken.Lexeme == Lexemes.TypeNewKeyWord)
                                {
                                    Epsilon();
                                    _lexer.RestorePosition(position);
                                    break;
                                }
                                else if (nextToken.Lexeme == Lexemes.TypeIdentifier)
                                {
                                    nextToken = _lexer.NextToken();
                                    if (nextToken.Lexeme == Lexemes.TypeOpenParenthesis)
                                    {
                                        Epsilon();
                                        _lexer.RestorePosition(position);
                                        break;
                                    }
                                }
                                _lexer.RestorePosition(position);
                                // если не eps, то вносим данные
                                _mag.Push(new SyntaxData() { IsTerminal = false, NonTerminal = NonTerminals.FieldAccess_1 });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeIdentifier, "") });
                                _mag.Push(new SyntaxData() { IsTerminal = true, Token = new Token(Lexemes.TypeDot, ".") });
                            }
                            else if (token.Lexeme == Lexemes.TypeIncrement
                                || token.Lexeme == Lexemes.TypeDecrement
                                || token.Lexeme == Lexemes.TypePlus
                                || token.Lexeme == Lexemes.TypeMinus
                                || token.Lexeme == Lexemes.TypeMult
                                || token.Lexeme == Lexemes.TypeDiv
                                || token.Lexeme == Lexemes.TypeMod
                                || token.Lexeme == Lexemes.TypeAssignmentSign)
                            {
                                Epsilon();
                            }
                            else
                            {
                                throw new SyntaxErrorException($"Ожидался символ: '.', но отсканирован символ: '{token.Value}'." +
                                    $"Строка: {_lexer.CurrentRow}, столбец: {_lexer.CurrentColumn}");
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

        /// <summary>
        /// Кладет в стэк либо <see cref="NonTerminals.MethodDeclaration"/>, либо <see cref="NonTerminals.FieldDeclaration"/>. <br></br>
        /// Указатель лексера должен быть на анализируемом имени. <br></br>
        /// Например, в строке: <code>int MethodName() { }</code> указатель лексера должен быть на 'MethodName', а в строке <code>int a;</code>
        /// указатель лексера должен быть на слове 'a'. <br></br>
        /// Не изменяет текущую позицию лексера.
        /// </summary>
        /// <exception cref="SyntaxErrorException"></exception>
        private NonTerminals ResolveMethodOrFieldDeclaration()
        {
            Lexer.Position position = _lexer.SavePosition();
            Token nextToken = _lexer.NextToken();
            NonTerminals result;
            if (nextToken == Token.Default())
                throw new SyntaxErrorException("Встречен конец файла");
            if (nextToken.Lexeme == Lexemes.TypeOpenParenthesis) // это метод
            {
                result = NonTerminals.MethodDeclaration;
            }
            else // иначе, поле
            {
               result = NonTerminals.FieldDeclaration;
            }
            _lexer.RestorePosition(position);
            return result;
        }
    }
}
