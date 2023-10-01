using GardenLightHyperionConnector.Manager;
using System.Collections;
using System.Diagnostics;
using nanoFramework.WebServer;
using Modicus.Web.Interfaces;
using Modicus.Commands.Interfaces;
using Modicus.Interfaces;

namespace Modicus.Web
{
    internal class ModicusWebpages
    {
        private readonly ISettingsManager settingsManager;
        private readonly ICommandManager commandManager;

        /// <summary>
        /// Creates a new ModicusWebpages instance.
        /// </summary>
        public ModicusWebpages(ISettingsManager settingsManager, ICommandManager commandManager)
        {
            this.settingsManager = settingsManager;
            this.commandManager = commandManager;
        }

        /// <summary>
        /// Serves the favicon
        /// </summary>
        /// <param name="e">Web server context</param>
        [Route("favicon.ico")]
        public void Favico(WebServerEventArgs e)
        {
            // WebServer.SendFileOverHTTP(e.Context.Response, "favico.ico", Resources.GetBytes(Resources.BinaryResources.favico), "image/ico");
        }

        [Route("style.css")]
        public void Style(WebServerEventArgs e)
        {
            e.Context.Response.ContentType = "text/css";
            WebServer.OutPutStream(e.Context.Response, Resources.Resources.GetString(Resources.Resources.StringResources.style));
        }

        /// <summary>
        /// Serves the SVG image
        /// </summary>
        /// <param name="e">Web server context</param>
        [Route("image.svg")]
        public void Image(WebServerEventArgs e)
        {
            //  WebServer.SendFileOverHTTP(e.Context.Response, "image.svg", Resources.GetBytes(Resources.BinaryResources.image), "image/svg+xml");
        }

        [Route("select_section")]
        public void SelectSettings(WebServerEventArgs e)
        {
            Debug.WriteLine(e.Context.Request.RawUrl);

            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);
            var ip_address = (string)hashPars["ip_address"];
            var mqtt_settings = (string)hashPars["mqtt_settings"];
            var wifi_settings = (string)hashPars["wifi_settings"];

            if (ip_address != null)
            {
                WebManager.OutPutResponse(e.Context.Response, Resources.Resources.GetString(Resources.Resources.StringResources.ip_settings));
                return;
            }

            if (mqtt_settings != null)
            {
                WebManager.OutPutResponse(e.Context.Response, CreateMQTTSettingsPage(""));
                return;
            }

            if (wifi_settings != null)
            {
                WebManager.OutPutResponse(e.Context.Response, Resources.Resources.GetString(Resources.Resources.StringResources.wifi_settings));
                return;
            }

            Default(e);
        }

        /// <summary>
        /// This is the default page
        /// </summary>
        /// <remarks>the / route *must* always be the last one and the last of the last controller passed
        /// to the constructor</remarks>
        /// <param name="e">Web server context</param>
        [Route("default.html"), Route("index.html"), Route("/")]
        public void Default(WebServerEventArgs e)
        {
            e.Context.Response.ContentType = "text/html";

            var status_message = "Welcome to Modicus ... Have fun!";
            var page = string.Format(Resources.Resources.GetString(Resources.Resources.StringResources.index), status_message);
            WebServer.OutPutStream(e.Context.Response, page);
        }

        public string CreateMQTTSettingsPage(string message)
        {
            var mqttSettings = settingsManager.GlobalSettings.MqttSettings;
            var page = string.Format(Resources.Resources.GetString(Resources.Resources.StringResources.mqtt_settings), message, 
                mqttSettings.MqttHostName,
                mqttSettings.MqttPort, 
                mqttSettings.MqttClientID,
                mqttSettings.SendInterval.TotalSeconds);
            return page;
        }
    }
}