using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using Modicus.Interfaces;
using Modicus.Manager;
using Modicus.Web;
using Modicus.Web.Interfaces;
using nanoFramework.WebServer;

namespace GardenLightHyperionConnector.Manager
{
    internal class WebManager : IWebManager
    {
        private IServiceProvider ServiceProvider { get; set; }

        public WebManager(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Start the web service
        /// </summary>
        public void StartWebManager()
        {
            using WebServer server = new WebServerDI(80, HttpProtocol.Http, new Type[] { typeof(ModicusWebpageAPI), typeof(ModicusWebpages) }, ServiceProvider);
            // Start the server.
            server.Start();
            Thread.Sleep(Timeout.Infinite);
        }

        public static Hashtable ParseParamsFromStream(Stream inputStream)
        {
            byte[] buffer = new byte[inputStream.Length];
            inputStream.Read(buffer, 0, (int)inputStream.Length);
            
            return ParseParams(HttpUtility.UrlDecode(System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length)));
        }

        public static Hashtable ParseParams(string rawParams)
        {
            Hashtable hash = new Hashtable();

            string[] parPairs = rawParams.Split('&');
            foreach (string pair in parPairs)
            {
                string[] nameValue = pair.Split('=');

                if (nameValue.Length >= 2)
                    hash.Add(nameValue[0], nameValue[1]);
            }
            return hash;
        }

        public static void OutPutResponse(HttpListenerResponse response, string responseString)
        {
            var responseBytes = System.Text.Encoding.UTF8.GetBytes(responseString);
            OutPutByteResponse(response, System.Text.Encoding.UTF8.GetBytes(responseString));
        }

        public static void OutPutByteResponse(HttpListenerResponse response, Byte[] responseBytes)
        {
            response.ContentLength64 = responseBytes.Length;
            response.OutputStream.Write(responseBytes, 0, responseBytes.Length);

        }
    }
}