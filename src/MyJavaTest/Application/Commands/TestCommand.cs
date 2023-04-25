using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.Application.Commands
{
    public abstract class TestCommand
    {
        protected virtual string GetText(string fileName)
        {
            using FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StringBuilder builder = new StringBuilder((int)stream.Length);
            char symbol;
            int @byte = -1;
            for (int i = 0; i < stream.Length; i++)
            {
                @byte = stream.ReadByte();
                if (@byte > -1)
                {
                    symbol = (char)@byte;
                    builder.Append(symbol);
                }
            }
            stream.Close();
            return builder.ToString();
        }

        public abstract void Execute(string fileName);
    }
}
