using MyJavaTest.WpfClient.Infrastructure.Commands;
using MyJavaTest.WpfClient.Services.Dialogs;
using MyJavaTest.WpfClient.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Windows.Input;
using System.Windows.Markup;

namespace MyJavaTest.WpfClient.ViewModels
{
    [MarkupExtensionReturnType(typeof(MainWindowViewModel))]
    public class MainWindowViewModel : ViewModel
    {
        private IFolderBrowserDialog _folderBrowserDialog;
        public MainWindowViewModel()
        {
            ChooseFolderCommand = new LambdaCommand(OnChooseFolderCommandExecuted, CanChooseFolderCommandExecute);
            TestSelectedFileCommand = new LambdaCommand(OnTestSelectedFileCommandExecuted, CanTestSelectedFileCommandExecute);
            TestFilesCommand = new LambdaCommand(OnTestFilesCommandExecuted, CanTestFilesCommandExecute);
            DropResultCommand = new LambdaCommand(OnDropResultCommandExecuted, CanDropResultCommandExecute);
            DropAllResultsCommand = new LambdaCommand(OnDropAllResultsCommandExecuted, CanDropAllResultsCommandExecute);
            ClearTableCommand = new LambdaCommand(OnClearTableCommandExecuted, CanClearTableCommandExecute);
            ReloadFilesCommand = new LambdaCommand(OnReloadFilesCommandExecuted, CanReloadFilesCommandExecute);
        }

        public MainWindowViewModel(IFolderBrowserDialog folderBrowserDialog) : this()
        {
            _folderBrowserDialog = folderBrowserDialog;
        }

        #region Properties
        public event EventHandler? DialogComplete;

        protected virtual void OnDialogComplete(EventArgs e) => DialogComplete?.Invoke(this, e);

        private string _title = "MyJavaTest";
        public string Title { get => _title; set => Set(ref _title, value); }

        private string _status = "Status";
        public string Status { get => _status; set => Set(ref _status, value); }

        private string _directoryPath = string.Empty;
        public string DirectoryPath { get => _directoryPath; set => Set(ref _directoryPath, value);}

        private string _fileContent = string.Empty;
        public string FileContent { get => _fileContent; set => Set(ref _fileContent, value); }

        private int _totalTested = 0;
        public int TotalTested { get => _totalTested; set => Set(ref _totalTested, value); }

        private int _testedWithoutErrors = 0;
        public int TestedWithoutErrors { get => _testedWithoutErrors; set => Set(ref _testedWithoutErrors, value); }

        private int _testedWithErrors = 0;
        public int TestedWithErrors { get => _testedWithErrors; set => Set(ref _testedWithErrors, value); }

        private bool _testVariantLexer = true;
        public bool TestVariantLexer
        {
            get => _testVariantLexer;
            set => Set(ref _testVariantLexer, value);
        }

        private bool _testVariantSyntax = false;
        public bool TestVariantSyntax
        {
            get => _testVariantSyntax;
            set => Set(ref _testVariantSyntax, value);
        }

        private bool _testVariantSemantic = false;
        public bool TestVariantSemantic
        {
            get => _testVariantSemantic;
            set => Set(ref _testVariantSemantic, value);
        }

        public ObservableCollection<TestFileViewModel> TestFiles { get; private set; } = new ObservableCollection<TestFileViewModel>();

        private TestFileViewModel _selectedFile = null;
        public TestFileViewModel SelectedFile { get => _selectedFile; set => Set(ref _selectedFile, value); }

        #endregion

        #region Commands
        #region ChooseFolderCommand
        public ICommand ChooseFolderCommand { get; }
        private void OnChooseFolderCommandExecuted(object p)
        {
            if (_folderBrowserDialog.ShowDialog() == true)
            {
                DirectoryPath = _folderBrowserDialog.SelectedPath;
            }
        }
        private bool CanChooseFolderCommandExecute(object p) => true;
        #endregion

        #region TestSelectedFileCommand
        public ICommand TestSelectedFileCommand { get; }
        private void OnTestSelectedFileCommandExecuted(object p)
        {
            
        }
        private bool CanTestSelectedFileCommandExecute(object p) => SelectedFile is not null;
        #endregion

        #region TestFilesCommand
        public ICommand TestFilesCommand { get; }
        private void OnTestFilesCommandExecuted(object p)
        {

        }
        private bool CanTestFilesCommandExecute(object p) => TestFiles.Any();
        #endregion

        #region DropResultCommand
        public ICommand DropResultCommand { get; }
        private void OnDropResultCommandExecuted(object p)
        {

        }
        private bool CanDropResultCommandExecute(object p) => SelectedFile is not null 
            && SelectedFile.TestStatus is not Models.TestStatus.NotTested;
        #endregion

        #region DropAllResultsCommand
        public ICommand DropAllResultsCommand { get; }
        private void OnDropAllResultsCommandExecuted(object p)
        {

        }
        private bool CanDropAllResultsCommandExecute(object p) => TestFiles.Any() 
            && TestFiles.FirstOrDefault(f => f.TestStatus == Models.TestStatus.NotTested) is not default(TestFileViewModel);
        #endregion

        #region ClearTableCommand
        public ICommand ClearTableCommand { get; }
        private void OnClearTableCommandExecuted(object p)
        {

        }
        private bool CanClearTableCommandExecute(object p) => TestFiles.Any();
        #endregion

        #region ReloadFilesCommand
        public ICommand ReloadFilesCommand { get; }
        private void OnReloadFilesCommandExecuted(object p)
        {

        }
        private bool CanReloadFilesCommandExecute(object p) => !string.IsNullOrWhiteSpace(DirectoryPath);
        #endregion
        #endregion
    }
}
