using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web;
using Modicus.Manager;
using Modicus.Manager.Interfaces;
using Modicus.Web;
using nanoFramework.WebServer;

namespace GardenLightHyperionConnector.Manager
{
    internal class WebManager : IWebManager
    {
        private IServiceProvider ServiceProvider { get; set; }
        private WebServer server;

        /// <summary>Initializes a new instance of the <see cref="WebManager"/> class.</summary>
        /// <param name="serviceProvider">The service provider.</param>
        public WebManager(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        /// <summary>Start the web service.</summary>
        public void StartWebManager()
        {
            server = new WebServerDI(80, HttpProtocol.Http, new Type[] { typeof(ModicusWebpageAPI), typeof(ModicusWebpages) }, ServiceProvider);
            // Start the server.
            server.Start();
            Debug.WriteLine("++++ WebServer started! ++++");
        }

        /// <summary>Parses the parameters from stream.</summary>
        /// <param name="inputStream">The input stream.</param>
        public static Hashtable ParseParamsFromStream(Stream inputStream)
        {
            byte[] buffer = new byte[inputStream.Length];
            inputStream.Read(buffer, 0, (int)inputStream.Length);

            return ParseParams(HttpUtility.UrlDecode(System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length)));
        }

        /// <summary>Parses the parameters from a string.</summary>
        /// <param name="rawParams">The raw parameters.</param>
        public static Hashtable ParseParams(string rawParams)
        {
            Hashtable hash = new();

            string[] parPairs = rawParams.Split('&');
            foreach (string pair in parPairs)
            {
                string[] nameValue = pair.Split('=');

                if (nameValue.Length >= 2)
                    hash.Add(nameValue[0], nameValue[1]);
            }
            return hash;
        }

        /// <summary>Creates the output response.</summary>
        /// <param name="response">The response.</param>
        /// <param name="responseString">The response string.</param>
        public static void OutPutResponse(HttpListenerResponse response, string responseString)
        {
            OutPutByteResponse(response, System.Text.Encoding.UTF8.GetBytes(responseString));
        }

        /// <summary>Writes the output response.</summary>
        /// <param name="response">The response.</param>
        /// <param name="responseBytes">The response bytes.</param>
        public static void OutPutByteResponse(HttpListenerResponse response, Byte[] responseBytes)
        {
            response.ContentLength64 = responseBytes.Length;
            response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
        }
    }
}