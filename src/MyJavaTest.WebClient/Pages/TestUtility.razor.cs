using MyJavaTest.WebClient.Models;
using MyJavaTest.WebClient.ViewModels;
using static MudBlazor.CategoryTypes;

namespace MyJavaTest.WebClient.Pages
{
    public partial class TestUtility
    {
        public string DirectoryPath { get; set; } = string.Empty;

        private bool _dense = false;
        private bool _hover = true;
        private bool _striped = false;
        private bool _bordered = false;
        private string _searchString = "";
        private TestFileViewModel _selectedFile = null;
        private HashSet<TestFileViewModel> TestFiles = new HashSet<TestFileViewModel>();

        private TestFileType TestFileType { get; set; } = TestFileType.LexerTest;

        private bool FilterFunction(TestFileViewModel test) => FilterFunc(test, _searchString);

        private bool FilterFunc(TestFileViewModel test, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;
            if (test.TestStatus.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (test.FileName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (test.TestFileType.GetName().Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if ($"{test.TestStatus} {test.FileName} {test.TestFileType.GetName()}".Contains(searchString))
                return true;
            return false;
        }

    }
}
