using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Web;
using GardenLightHyperionConnector.Manager;
using Modicus.Commands.Interfaces;
using Modicus.Interfaces;
using nanoFramework.WebServer;

namespace Modicus.Web
{
    //[Authentication("Basic:user p@ssw0rd")]
    internal class ModicusWebpageAPI
    {
        private readonly ISettingsManager settingsManager;
        private readonly ICommandManager commandManager;

        /// <summary>
        /// Creates a new ModicusWebpageAPI instance.
        /// </summary>
        public ModicusWebpageAPI(ISettingsManager settingsManager, ICommandManager commandManager)
        {
            this.settingsManager = settingsManager;
            this.commandManager = commandManager;
        }

        [Route("wifi_settings")]
        public void WifiSettings(WebServerEventArgs e)
        {
            Debug.WriteLine(e.Context.Request.RawUrl);

            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);
            var ssid = (string)hashPars["ssid"];
            var password = (string)hashPars["password"];

            Debug.WriteLine($"Wireless parameters SSID:{ssid} PASSWORD:{password}");
        }

        [Route("ip_settings")]
        public void IPSettings(WebServerEventArgs e)
        {
            Debug.WriteLine(e.Context.Request.RawUrl);

            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);
            var ip = (string)hashPars["ip-address"];
            var subnet = (string)hashPars["subnetmask"];
            var gateway = (string)hashPars["default-gateway"];

            Debug.WriteLine($"IP parameters IP:{ip} Subnetmask:{subnet} Default Gateway {gateway}");
        }

        [Route("mqtt_settings")]
        public void MqttSettings(WebServerEventArgs e)
        {
            Debug.WriteLine(e.Context.Request.RawUrl);

            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);
            var clientID = (string)hashPars["mqtt-clientid"];
            var sendInterval = (string)hashPars["mqtt-send-interval"];
            var save = (string)hashPars["save"];
            var back = (string)hashPars["back"];

            Debug.WriteLine($"MQTT parameters ClientID:{clientID} Send Interval:{sendInterval}");

            string message = "";
            if (save != null)
            {
                commandManager.CmdMQTTClientID.Execute(new Commands.CmdMqttClientIdData { ClientID = clientID });

                if (int.TryParse(sendInterval, out var result))
                {
                    if (result >= 1)
                        settingsManager.GlobalSettings.MqttSettings.SendInterval = TimeSpan.FromSeconds(result);
                }
                message = $"New: MQTT parameters: ClientID:{clientID} Send Interval:{settingsManager.GlobalSettings.MqttSettings.SendInterval}.";
            }
            if (back != null)
            {
                e.Context.Response.RedirectLocation = HttpUtility.UrlDecode("index.html");
            }

            var page = string.Format(Resources.Resources.GetString(Resources.Resources.StringResources.mqtt_settings), message);
            WebManager.OutPutResponse(e.Context.Response, page);
        }

        [Route("select_section")]
        public void SelectSettings(WebServerEventArgs e)
        {
            Debug.WriteLine(e.Context.Request.RawUrl);

            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);
            var ip_address = (string)hashPars["ip_address"];
            var mqtt_settings = (string)hashPars["mqtt_settings"];
            var bwifi_settings = (string)hashPars["wifi_settings"];

            if (ip_address != null)
                WebManager.OutPutResponse(e.Context.Response, Resources.Resources.GetString(Resources.Resources.StringResources.ip_settings));

            if (mqtt_settings != null)
            {
                var page = string.Format(Resources.Resources.GetString(Resources.Resources.StringResources.mqtt_settings), "");
                WebManager.OutPutResponse(e.Context.Response, page);
            }

            if (bwifi_settings != null)
                WebManager.OutPutResponse(e.Context.Response, Resources.Resources.GetString(Resources.Resources.StringResources.wifi_settings));
        }
    }
}