using System;
using System.Collections.Generic;
using System.Text;
using nanoFramework.WebServer;

namespace Modicus.Web.Interfaces
{
    internal interface IModicusWebpages
    {
        [Route("style.css")]
        public void Style(WebServerEventArgs e);

        [Route("select_section")]
        void SelectSettings(WebServerEventArgs e);

        [Route("default.html"), Route("index.html"), Route("/")]
        void Default(WebServerEventArgs e);
    }
}
