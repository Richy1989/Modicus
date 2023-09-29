using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using nanoFramework.WebServer;

namespace Modicus.Web
{
    [Authentication("Basic:user p@ssw0rd")]
    public class ModicusWebpageAPI
    {
        /// <summary>
        /// Stop motor
        /// </summary>
        /// <param name="e">Web server context</param>
        [Route("save")]
        public void SaveSettings(WebServerEventArgs e)
        {
            Debug.WriteLine(e.Context.Request.RawUrl);
        }
    }
}
