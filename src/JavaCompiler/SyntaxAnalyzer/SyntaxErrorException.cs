using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaCompiler.SyntaxAnalyzer
{
    public class SyntaxErrorException : Exception
    {
        public SyntaxErrorException(string message) : base(message) { }
    }
}
