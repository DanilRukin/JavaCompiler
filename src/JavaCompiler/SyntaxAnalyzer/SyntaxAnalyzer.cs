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
                        throw new SyntaxErrorException($"Ожидался символ: '{data.Token.Value}', но отсканирован символ: '{token.Value}'.");
                    }
                }
            }
        }
    }
}
