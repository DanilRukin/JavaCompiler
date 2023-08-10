using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.WpfClient.Services.Dialogs
{
    public interface IFolderBrowserDialog
    {
        string SelectedPath { get; set; }
        bool ShowDialog();
    }
}
