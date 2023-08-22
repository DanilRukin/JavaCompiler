using JavaCompiler.Common;
using JavaCompiler.LexerAnalyzer;
using JavaCompiler.SyntaxAnalyzer;
using MediatR;
using MyJavaTest.WpfClient.Infrastructure.Commands;
using MyJavaTest.WpfClient.Models.Services.Interfaces;
using MyJavaTest.WpfClient.Services.Dialogs;
using MyJavaTest.WpfClient.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;
using static System.Net.Mime.MediaTypeNames;

namespace MyJavaTest.WpfClient.ViewModels
{
    [MarkupExtensionReturnType(typeof(MainWindowViewModel))]
    public class MainWindowViewModel : ViewModel
    {
        private IFolderBrowserDialog _folderBrowserDialog;
        private ITestFactory _testFactory;
        private Lexer _lexer;
        private SyntaxAnalyzer _syntaxAnalyzer;
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

        public MainWindowViewModel(IFolderBrowserDialog folderBrowserDialog, ITestFactory testFactory, SyntaxAnalyzer syntaxAnalyzer, Lexer lexer) : this()
        {
            _folderBrowserDialog = folderBrowserDialog;
            _testFactory = testFactory;
            _syntaxAnalyzer = syntaxAnalyzer;
            _lexer = lexer;
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
                LoadTestsAsync();
            }
        }
        private bool CanChooseFolderCommandExecute(object p) => true;
        #endregion

        #region TestSelectedFileCommand
        public ICommand TestSelectedFileCommand { get; }
        private void OnTestSelectedFileCommandExecuted(object p)
        {
            if (SelectedFile.TestFileType == Models.TestFileType.LexerTest)
                ExecuteLexerTest(SelectedFile);
            else if (SelectedFile.TestFileType == Models.TestFileType.SyntaxTest)
                ExecuteSyntaxTest(SelectedFile);
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


        private void LoadTestsAsync()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(DirectoryPath))
                {
                    if (Directory.Exists(DirectoryPath))
                    {
                        TestFiles.Clear();
                        var files = Directory.GetFiles(DirectoryPath);
                        var tests = _testFactory.GetTests(files);
                        foreach (var test in tests)
                        {
                            TestFiles.Add(test);
                            Status = $"Тестов добавлено: {TestFiles.Count}";
                        }
                    }
                }
                Status = "Добавление тестов завершено!";
            }
            catch (Exception e)
            {
                Status = $"Ошибка при загрузке: {e.Message}";
            }
            
        }

        private void ExecuteLexerTest(TestFileViewModel test)
        {
            try
            {
                Status = $"Тестирую файл {test.FileName}";
                string text = test.FileContent;
                _lexer.SetText(text);
                Token token;
                while ((token = _lexer.NextToken()).Lexeme != Lexemes.TypeEnd)
                {
                    test.TestLog.Add($"Lexeme type is: {token.Lexeme};\tLexeme value is: {token.Value}\r\n");
                }
                test.TestLog.Add($"Lexeme type is: {token.Lexeme};\tLexeme value is: {token.Value}\r\n");
                _lexer.ClearText();
                TotalTested++;
                TestedWithoutErrors++;
                Status = $"Файл {test.FileName} протестирован успешно.";
            }
            catch (Exception e)
            {
                test.TestLog.Add($"Error: {e.Message}");
                test.TestStatus = Models.TestStatus.Error;
                test.TestStatusColor = Color.Red;
                TotalTested++;
                TestedWithErrors++;
                _lexer.ClearText();
                Status = $"Файл {test.FileName} протестирован с ошибками.";
            }
        }

        private void ExecuteSyntaxTest(TestFileViewModel test)
        {
            try
            {
                Status = $"Тестирую файл {test.FileName}";
                string text = test.FileContent;
                _syntaxAnalyzer.SetText(text);
                _syntaxAnalyzer.Analyze();
                _syntaxAnalyzer.ClearText();
                TotalTested++;
                TestedWithoutErrors++;
                Status = $"Файл {test.FileName} протестирован успешно.";
            }
            catch (Exception e)
            {
                test.TestLog.Add($"Error: {e.Message}");
                test.TestStatus = Models.TestStatus.Error;
                test.TestStatusColor = Color.Red;
                TotalTested++;
                TestedWithErrors++;
                _syntaxAnalyzer.ClearText();
                Status = $"Файл {test.FileName} протестирован с ошибками.";
            }
        }
    }
}
