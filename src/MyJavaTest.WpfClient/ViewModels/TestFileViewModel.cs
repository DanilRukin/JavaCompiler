using MyJavaTest.WpfClient.Models;
using MyJavaTest.WpfClient.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.WpfClient.ViewModels
{
    public class TestFileViewModel : ViewModel
    {
        private string _fileName = string.Empty;
        public string FileName { get => _fileName; set => Set(ref _fileName, value); }

        private TestFileType _testFileType = TestFileType.Unknown;
        public TestFileType TestFileType { get => _testFileType; set => Set(ref _testFileType, value); }

        private TestStatus _testStatus = TestStatus.NotTested;
        public TestStatus TestStatus { get => _testStatus; set => Set(ref _testStatus, value); }

        private Color _testStatusColor = DefaultColor;
        public Color TestStatusColor { get => _testStatusColor; set => Set(ref _testStatusColor, value); }

        private string _fileContent = string.Empty;
        public string FileContent { get => _fileContent; set => Set(ref _fileContent, value); }

        public ObservableCollection<TestLogItemViewModel> TestLog { get; private set; } = new ObservableCollection<TestLogItemViewModel>();

        public static Color DefaultColor => Color.Black;
    }
}
