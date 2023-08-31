using MyJavaTest.WpfClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.WpfClient.Models.Services.Interfaces
{
    public interface ITestFactory
    {
        TestFileViewModel GetTest(string fileName);
        IEnumerable<TestFileViewModel> GetTests(IEnumerable<string> fileNames);
    }
}
