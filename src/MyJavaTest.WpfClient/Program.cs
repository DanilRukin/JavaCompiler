using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyJavaTest.WpfClient
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
