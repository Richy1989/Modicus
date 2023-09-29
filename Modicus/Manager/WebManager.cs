using System;
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
    }
}