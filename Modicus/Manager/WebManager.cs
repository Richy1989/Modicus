using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading;
using Modicus.Web;
using nanoFramework.WebServer;

namespace GardenLightHyperionConnector.Manager
{
    internal class WebManager
    {
        public void StartWebManager()
        {
            using (WebServer server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(ModicusWebpageAPI), typeof(ModicusWebpages) }))
            {
                // To test authentication with various scenarios
                server.Credential = new NetworkCredential("user", "password");
                // Add a handler for commands that are received by the server

                // Start the server.
                server.Start();

                Thread.Sleep(Timeout.Infinite);
            }
        }

        public static Hashtable ParseParamsFromStream(Stream inputStream)
        {
            byte[] buffer = new byte[inputStream.Length];
            inputStream.Read(buffer, 0, (int)inputStream.Length);

            return ParseParams(System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length));
        }

        public static Hashtable ParseParams(string rawParams)
        {
            Hashtable hash = new Hashtable();

            string[] parPairs = rawParams.Split('&');
            foreach (string pair in parPairs)
            {
                string[] nameValue = pair.Split('=');
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