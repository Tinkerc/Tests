using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace cefsharp.Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            string url = "http://www.zhaogang.com";
            webBrowser1.Url = new Uri(url);
        }
    }
}
