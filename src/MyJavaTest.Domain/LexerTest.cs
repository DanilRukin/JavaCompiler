﻿using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.Domain
{
    public class LexerTest : IRequest
    {
        public string Content { get; }
        public string FileName { get; }

        public LexerTest(string content, string fileName)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }
    }
}
