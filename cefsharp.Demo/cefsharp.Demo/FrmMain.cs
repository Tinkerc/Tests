using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CefSharp.WinForms;
using CefSharp;

namespace cefsharp.Demo
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();          
        }

        private void AddTab(string url, int? insertIndex = null)
        {
            browserTabControl.SuspendLayout();

            var browser = new ChromiumWebBrowser(url)
            {
                Dock = DockStyle.Fill,
            };

            var tabPage = new TabPage(url)
            {
                Dock = DockStyle.Fill
            };

            //This call isn't required for the sample to work. 
            //It's sole purpose is to demonstrate that #553 has been resolved.
            browser.CreateControl();

            tabPage.Controls.Add(browser);

            if (insertIndex == null)
            {
                tabPage.ToolTipText = browser.Title;
                browserTabControl.TabPages.Add(tabPage);
            }
            else
            {
                browserTabControl.TabPages.Insert(insertIndex.Value, tabPage);
            }

            //Make newly created tab active
            browserTabControl.SelectedTab = tabPage;

            browserTabControl.ResumeLayout(true);
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            
            // webCom.Load(textBox1.Text);
        }

        private void newTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddTab("www.baidu.com");
        }

        private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (browserTabControl.Controls.Count == 0)
            {
                return;
            }

            var currentIndex = browserTabControl.SelectedIndex;

            var tabPage = browserTabControl.Controls[currentIndex];

            var control = GetCurrentTabControl();
            if (control != null)
            {
                control.Dispose();
            }

            browserTabControl.Controls.Remove(tabPage);

            browserTabControl.SelectedIndex = currentIndex - 1;

            if (browserTabControl.Controls.Count == 0)
            {
                //ExitApplication();
            }
        }

        private ChromiumWebBrowser GetCurrentTabControl()
        {
            if (browserTabControl.SelectedIndex == -1)
            {
                return null;
            }

            var tabPage = browserTabControl.Controls[browserTabControl.SelectedIndex];
            var control = (ChromiumWebBrowser)tabPage.Controls[0];

            return control;
        }

        private void showDevToolsMenuItem_Click(object sender, EventArgs e)
        {
            
        }
    }
}
