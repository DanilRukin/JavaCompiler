using MyJavaTest.WpfClient.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.WpfClient.ViewModels
{
    public class TestFileViewModel : ViewModel
    {
        private string _fileName;
        public string FileName { get => _fileName; set => Set(ref _fileName, value); }

        private string _testFileType;
        public string TestFileType { get => _testFileType; set => Set(ref _testFileType, value); }

        private string _testStatus;
        public string TestStatus { get => _testStatus; set => Set(ref _testStatus, value); }

        private Color _testStatusColor = Color.Black;
        public Color TestStatusColor { get => _testStatusColor; set => Set(ref _testStatusColor, value); }
    }
}
