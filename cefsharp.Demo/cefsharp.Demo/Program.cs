using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CefSharp.WinForms.Example.Minimal;

namespace cefsharp.Demo
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            CefSharp.Cef.Initialize();  
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TabulationDemoForm());
        }
    }
}
