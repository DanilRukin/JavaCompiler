using MyJavaTest.WebClient.Models;

namespace MyJavaTest.WebClient.ViewModels
{
    public class TestFileViewModel
    {
        public string FileName { get; set; } = string.Empty;
        public TestFileType TestFileType { get; set; } = TestFileType.Unknown;

        public TestStatus TestStatus { get; set; } = TestStatus.Success;

        public string FileContent { get; set; } = string.Empty;

        public HashSet<TestLogItemViewModel> TestLog { get; private set; } = new HashSet<TestLogItemViewModel>();
    }
}
