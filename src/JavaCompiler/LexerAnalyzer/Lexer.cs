using JavaCompiler.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaCompiler.LexerAnalyzer
{
    public class Lexer
    {
        public int CurrentRow { get; set; } = 0;
        public int CurrentColumn { get; set; } = 0;
        private int _currentPositionInText = 0;
        private string _text = string.Empty;

        public class Position
        {
            private int _currentRow;
            private int _currentColumn;
            private int _currentPosition;
            private Lexer _lexer;
            public Position(Lexer lexer)
            {
                _lexer = lexer;
                _currentPosition = lexer._currentPositionInText;
                _currentColumn = lexer.CurrentColumn;
                _currentRow = lexer.CurrentRow;
            }
            public void Restore()
            {
                _lexer._currentPositionInText = _currentPosition;
                _lexer.CurrentColumn = _currentColumn;
                _lexer.CurrentRow = _currentRow;
            }
        }

        public Position SavePosition()
        {
            return new Position(this);
        }
        public void RestorePosition(Position position)
        {
            position.Restore();
        }

        public void Open(string fileName)
        {
            using FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            CurrentRow = 0;
            CurrentColumn = 0;
            StringBuilder builder = new StringBuilder((int)stream.Length);
            char symbol;
            int @byte = -1;
            for (int i = 0; i < stream.Length; i++)
            {
                @byte = stream.ReadByte();
                if (@byte > -1)
                {
                    symbol = (char)stream.ReadByte();
                    builder.Append(symbol);
                }               
            }
            _text = builder.ToString();
            stream.Close();
        }

        public void Close()
        {
            _text = string.Empty;
            CurrentColumn = 0;
            CurrentRow = 0;
        }

        public Token NextToken()
        {
            return null;
        }

        private bool IsDigit(char symbol) => char.IsDigit(symbol);
        private bool IsLetter(char symbol) => char.IsLetter(symbol);
    }
}
