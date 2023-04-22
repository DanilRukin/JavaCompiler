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
    }
}
