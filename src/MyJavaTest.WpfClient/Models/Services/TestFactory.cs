using Microsoft.Extensions.Configuration;
using MyJavaTest.WpfClient.Models.Services.Interfaces;
using MyJavaTest.WpfClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.WpfClient.Models.Services
{
    public class TestFactory : ITestFactory
    {
        private IConfiguration _configuration;

        public TestFactory(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public TestFileViewModel GetTest(string fileName)
        {
            string lexerFileExtension = _configuration["LexerExt"];
            string syntaxFileExtension = _configuration["SyntaxExt"];
            string semanticFileExtension = _configuration["SyntaxExt"];
            string fileExt = fileName.Substring(fileName.LastIndexOf('.') + 1);
            TestFileViewModel model = new()
            {
                FileName = fileName,
                TestStatus = TestStatus.NotTested,
                TestStatusColor = Color.Black
            };
            if (fileExt == syntaxFileExtension)
            {
                model.TestFileType = TestFileType.SyntaxTest;
            }
            else if (fileExt == lexerFileExtension)
            {
                model.TestFileType = TestFileType.LexerTest;               
            }
            else if (fileExt == semanticFileExtension)
            {
                model.TestFileType = TestFileType.SemanticTest;
            }
            model.FileContent = File.ReadAllText(fileName);
            return model;
        }

        public IEnumerable<TestFileViewModel> GetTests(IEnumerable<string> fileNames)
        {
            List<TestFileViewModel> tests = new List<TestFileViewModel>(fileNames.Count());
            foreach (var fileName in fileNames)
            {
                tests.Add(GetTest(fileName));
            }
            return tests;
        }
    }
}
