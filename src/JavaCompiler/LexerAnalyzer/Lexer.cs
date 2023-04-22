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
        public int CurrentRow { get; private set; } = 0;
        public int CurrentColumn { get; private set; } = 0;
        private int _currentPositionInText = 0;
        private string _text = string.Empty;
        private Token _token = Token.Default();
        private char _symbol;
        private const int LEXEME_DEFAULT_LENGTH = 100;
        private StringBuilder _lexeme = new StringBuilder(LEXEME_DEFAULT_LENGTH);
        private bool EndOfFile => _currentPositionInText >= _text?.Length;

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

        public void SetText(string text)
        {
            CurrentColumn = 0;
            CurrentRow = 0;
            _text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public void ClearText()
        {
            CurrentColumn = 0;
            CurrentRow = 0;
            _text = string.Empty;
        }

        public Position SavePosition()
        {
            return new Position(this);
        }
        public void RestorePosition(Position position)
        {
            position.Restore();
        }

        public Token NextToken()
        {
            _token = Token.Default();
            if (!EndOfFile)
            {
                SkipIgnorableSymbols();
                if (!EndOfFile)
                    ScanLexeme();
            }
            return _token;
        }
        /// <summary>
        /// Пропускает все игнорируемые символы и устанавливает _currentPositionInText на первый 'нормальный' символ
        /// </summary>
        private void SkipIgnorableSymbols()
        {
            while (!EndOfFile)
            {
                SkipSimpleIgnorableSymbols();  // сначала простые символы
                _symbol = _text[_currentPositionInText];
                if (_symbol == '/')  // комментарий, операция деления
                {
                    CurrentColumn++;
                    _currentPositionInText++;  // увеличить CurrentColumn и/или CurrentRow вдальнейшем
                    if (EndOfFile) // операция деления, откатываемся до '/'
                    {
                        _currentPositionInText--;
                        return;
                    }
                    _symbol = _text[_currentPositionInText];
                    if (_symbol == '/') // простой комментарий
                    {
                        CurrentColumn++;

                        _currentPositionInText++; // увеличить CurrentColumn и/или CurrentRow вдальнейшем
                        while (!EndOfFile)
                        {
                            _symbol = _text[_currentPositionInText];
                            if (_symbol == '\t')
                                CurrentColumn += 4;
                            else if (_symbol == '\r')
                                CurrentColumn = 0;
                            else if (_symbol == '\n')
                            {
                                CurrentRow++;  // переносимся на след. строку
                                break;  // и идем читать дальше
                            }
                            else
                                CurrentColumn++;  // пропускаем символ
                            _currentPositionInText++;
                        }
                    }
                    else if (_symbol == '*')  // сложный комментарий
                    {
                        CurrentColumn++;

                        _currentPositionInText++; // увеличить CurrentColumn и/или CurrentRow вдальнейшем
                        bool endOfDifficultComment = false;
                        while (!EndOfFile)
                        {
                            SkipSimpleIgnorableSymbols();
                            if (_symbol == '*')  // пропускаем, возможно, конечная *, а может и нет
                            {
                                CurrentColumn++;

                                _currentPositionInText++;
                                while (!EndOfFile)
                                {
                                    _symbol = _text[_currentPositionInText];
                                    if (_symbol == '*')
                                    {
                                        CurrentColumn++;
                                        _currentPositionInText++;
                                        continue;
                                    }
                                    else if (_symbol == '/')
                                    {
                                        CurrentColumn++;
                                        endOfDifficultComment = true;
                                        break;
                                        // выйти на самый внешний цикл
                                    }
                                    else
                                        break;  // иначе на предыдущее состояние, т.е. на внешний цикл
                                }
                            }
                            else
                            {
                                CurrentColumn++;  // т.к. мы встретили букву или цифру, или /
                                _currentPositionInText++;
                                continue;
                            }
                            if (endOfDifficultComment)
                                break;
                            _currentPositionInText++;
                        }
                    }
                    else  // операция деления, откатываемся до '/'
                    {
                        _currentPositionInText--;
                        return;
                    }
                }
                else
                    return;  // встретили символ
                _currentPositionInText++;
            }
        }
        /// <summary>
        /// Пропускает простые игнорируемые символы, такие как ' ', '\t', '\n', '\r' и устанавливает _currentPositionInText
        /// на первый 'неизвестный' символ
        /// </summary>
        private void SkipSimpleIgnorableSymbols()
        {
            while (!EndOfFile)
            {
                _symbol = _text[_currentPositionInText];
                switch (_symbol)
                {
                    case '\t':
                        CurrentColumn += 4;
                        break;
                    case '\n':
                        CurrentRow++;
                        break;
                    case '\r':
                        CurrentColumn = 0;
                        break;
                    case ' ':
                        CurrentColumn++;
                        break;
                    default:
                        return;
                }
                _currentPositionInText++;
            }
        }

        /// <summary>
        /// Сканиурет любую допустимую грамматикой лексему при условии, что все игнорируемые символы пропущены.
        /// </summary>
        private void ScanLexeme()
        {
            _lexeme.Clear();  // очистили поле лексемы
            _symbol = _text[_currentPositionInText];
            if (IsLetter(_symbol))
            {
                ProcessIdentifier();
            }
            else if (IsDigit(_symbol))
            {
                ProcessDigit();
            }
            else if (_symbol == '>')
            {
                _token.Lexeme = Lexemes.TypeMoreSign;
                _token.Value = _symbol.ToString();
                _lexeme.Append(_symbol);
                TryScanNext('=', Lexemes.TypeMoreOrEqualSign);
                _currentPositionInText++;
            }
            else if (_symbol == '<')
            {
                _token.Lexeme = Lexemes.TypeLessSign;
                _token.Value = _symbol.ToString();
                _lexeme.Append(_symbol);
                TryScanNext('=', Lexemes.TypeLessOrEqualSign);
                _currentPositionInText++;
            }
            else if (_symbol == '=')
            {
                _token.Lexeme = Lexemes.TypeAssignmentSign;
                _token.Value = _symbol.ToString();
                _lexeme.Append(_symbol);
                TryScanNext('=', Lexemes.TypeEqualSign);
                _currentPositionInText++;
            }
            else if (_symbol == '!')
            {
                _token.Value = "!";
                _token.Lexeme = Lexemes.TypeError;
                _currentPositionInText++;
                CurrentColumn++;
                if (EndOfFile)
                {
                    return;
                }
                _symbol = _text[_currentPositionInText];
                if (_symbol == '=')
                {
                    _currentPositionInText++;
                    _token.Lexeme = Lexemes.TypeNotEqualSign;
                    _token.Value = "!=";
                }
                else
                {
                    _token.Value = _symbol.ToString();
                    _token.Lexeme = Lexemes.TypeError;
                    _token.Value = $"Одиночно стоящий символ '!'";
                    return;
                }
            }
            else if (_symbol == '+')
            {
                _token.Lexeme = Lexemes.TypePlus;
                _token.Value = _symbol.ToString();
                _lexeme.Append(_symbol);
                TryScanNext('+', Lexemes.TypeIncrement);
                _currentPositionInText++;
            }
            else if (_symbol == '-')
            {
                _token.Lexeme = Lexemes.TypeMinus;
                _token.Value = _symbol.ToString();
                _lexeme.Append(_symbol);
                TryScanNext('-', Lexemes.TypeDecrement);
                _currentPositionInText++;
            }
            else
            {
                
                _token.Value = _symbol.ToString();
                bool success = Collections.Words.TryGetValue(_token.Value, out Lexemes lexeme);
                if (!success)
                {
                    _token.Value = $"Неожиданный символ: '{_token.Value}'";
                    _token.Lexeme = Lexemes.TypeError;
                }                   
                else
                    _token.Lexeme = lexeme;
                _currentPositionInText++;
            }
            return;
        }

        /// <summary>
		/// Обработка идентификатора
		/// </summary>
		private void ProcessIdentifier()
        {
            _lexeme.Append(_symbol);
            _currentPositionInText++;
            CurrentColumn++;
            while (!EndOfFile)
            {
                _symbol = _text[_currentPositionInText];
                if (IsDigit(_symbol) || IsLetter(_symbol))
                {
                    _lexeme.Append(_symbol);
                    _currentPositionInText++;
                    CurrentColumn++;
                }
                else
                {
                    _token.Value = _lexeme.ToString();
                    _token.Lexeme = Collections.GetLexemeByName(_token.Value);
                    return;
                }
            }
            // сюда попадем только в том случае, когда достигнут конец всего текста
            _token.Value = _lexeme.ToString();
            _token.Lexeme = Collections.GetLexemeByName(_token.Value);
            return;
        }

        /// <summary>
		/// Сканирует следующий символ, которым предположительно должен быть nextSymbolShouldBe, если отсканирован другой 
		/// символ, то ничего не делает
		/// </summary>
		/// <param name="nextSymbolShouldBe">символ, который, возможно, будет частью лексемы</param>
		/// <param name="nextLexemeShouldBe"></param>
		private bool TryScanNext(char nextSymbolShouldBe, Lexemes nextLexemeShouldBe)
        {
            _currentPositionInText++;
            CurrentColumn++;
            if (EndOfFile || (_symbol = _text[_currentPositionInText]) != nextSymbolShouldBe)
            {
                _currentPositionInText--;
                CurrentColumn--;
                return false;
            }
            else
            {
                _lexeme.Append(_symbol);
                _token.Lexeme = nextLexemeShouldBe;
                _token.Value = _lexeme.ToString();
                return true;
            }
        }

        /// <summary>
		/// Обработка числа. Сканирование ведется в два этапа, т.к. число типа double состоит из двух чисел типа int и точки
		/// </summary>
		private void ProcessDigit()
        {
            string value;
            value = ScanInteger();  // сканируем int 
            _token.Value = value;
            _token.Lexeme = Lexemes.TypeIntLiteral;
            if (EndOfFile)
            {
                return;
            }
            else
            {
                _symbol = _text[_currentPositionInText];
                if (_symbol == '.')  // если встретили точку
                {
                    _currentPositionInText++;
                    CurrentColumn++;
                    _token.Value += ".";
                    if (EndOfFile)
                    {
                        _token.Lexeme = Lexemes.TypeError;
                        return;
                    }
                    _symbol = _text[_currentPositionInText];
                    if (!IsDigit(_symbol))
                    {
                        _token.Value += _symbol;
                        _token.Lexeme = Lexemes.TypeError;
                        _currentPositionInText++;
                        CurrentColumn++;
                        return;
                    }
                    value = ScanInteger();  // то сканируем второую часть числа в виде int
                    _token.Value += value;
                    _token.Lexeme = Lexemes.TypeDoubleLiteral;
                    return;
                }
            }
        }

        /// <summary>
		/// Возвращает строку, представляющую число типа int, начинающееся с _symbol
		/// </summary>
		/// <returns></returns>
		private string ScanInteger()
        {
            _lexeme.Clear();
            _lexeme.Append(_symbol);
            _currentPositionInText++;
            CurrentColumn++;
            while (_currentPositionInText < _text.Length)
            {
                _symbol = _text[_currentPositionInText];
                if (IsDigit(_symbol))
                {
                    _lexeme.Append(_symbol);
                    _currentPositionInText++;
                    CurrentColumn++;
                }
                else
                {
                    return _lexeme.ToString();
                }
            }
            // если достигнут конец текста либо в процессе чтения числа, либо даже если в цикл не заходили
            return _lexeme.ToString();
        }

        private bool IsDigit(char symbol) => char.IsDigit(symbol);
        private bool IsLetter(char symbol) => char.IsLetter(symbol);
    }
}
