using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.WpfClient.Services.Dialogs
{
    public class FolderBrowserDialog : IFolderBrowserDialog
    {
        private const string CHOOSE_FOLDER = "Выбор папки";
        private string _parentDirectory = Environment.CurrentDirectory;
        public string ParentDirectory { get => _parentDirectory; set => _parentDirectory = value; }
        private string _selectedPath = string.Empty;
        public string SelectedPath { get => _selectedPath; set => _selectedPath = value; }

        public bool ShowDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = ParentDirectory;
            dialog.Filter = "Только папки|*.";
            dialog.ValidateNames = false;
            dialog.CheckFileExists = false;
            dialog.FileName = CHOOSE_FOLDER;
            if (dialog.ShowDialog() == true)
            {
                SelectedPath = dialog.FileName.Substring(0, dialog.FileName.Length - CHOOSE_FOLDER.Length);
                ParentDirectory = SelectedPath;
                return true;
            }
            return false;
        }
    }
}
