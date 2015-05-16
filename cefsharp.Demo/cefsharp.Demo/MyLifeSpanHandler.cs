using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;

namespace cefsharp.Demo
{
    public class MyLifeSpanHandler : ILifeSpanHandler
    {
        public MyLifeSpanHandler()
        {

        }

        public bool OnBeforePopup(IWebBrowser browser, string url, ref int x, ref int y, ref int width, ref int height)
        {
            return true;
        }

        public void OnBeforeClose(IWebBrowser browser)
        {
            throw new NotImplementedException();
        }
    }
}
