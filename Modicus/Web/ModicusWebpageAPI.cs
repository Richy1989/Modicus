using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using GardenLightHyperionConnector.Manager;
using Modicus.Commands.Interfaces;
using Modicus.Interfaces;
using Modicus.Web.Interfaces;
using nanoFramework.M2Mqtt;
using nanoFramework.WebServer;

namespace Modicus.Web
{
    //[Authentication("Basic:user p@ssw0rd")]
    internal class ModicusWebpageAPI
    {
        private readonly ISettingsManager settingsManager;
        private readonly ICommandManager commandManager;
        private readonly ModicusWebpages modicusWebpages;

        /// <summary>
        /// Creates a new ModicusWebpageAPI instance.
        /// </summary>
        public ModicusWebpageAPI(ISettingsManager settingsManager, ICommandManager commandManager, ModicusWebpages modicusWebpages)
        {
            this.settingsManager = settingsManager;
            this.commandManager = commandManager;
            this.modicusWebpages = modicusWebpages;
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

        private Thread mqttRestart;
        [Route("mqtt_settings")]
        public void MqttSettings(WebServerEventArgs e)
        {
            Debug.WriteLine(e.Context.Request.RawUrl);

            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);
            var clientID = (string)hashPars["mqtt-clientid"];
            var sendInterval = (string)hashPars["mqtt-send-interval"];
            var ip = (string)hashPars["mqtt-server-ip"];
            var port = (string)hashPars["mqtt-server-port"];
            var save = (string)hashPars["save"];
            var back = (string)hashPars["back"];

            string message = "";
            if (save != null)
            {
                //commandManager.CmdMQTTClientID.Execute(new Commands.CmdMqttClientIdData { ClientID = clientID });

                
                settingsManager.GlobalSettings.MqttSettings.MqttPort = int.TryParse(port, out var resultPort) ? resultPort : settingsManager.GlobalSettings.MqttSettings.MqttPort;
                settingsManager.GlobalSettings.MqttSettings.MqttClientID = clientID;

                if (int.TryParse(sendInterval, out var result))
                {
                    if (result >= 1)
                        settingsManager.GlobalSettings.MqttSettings.SendInterval = TimeSpan.FromSeconds(result);
                }
                else
                    message = $"Send Interval not Valid!\n{message}";
               
                try
                {
                    IPAddress.Parse(ip);
                    settingsManager.GlobalSettings.MqttSettings.MqttHostName = ip;
                }
                catch
                {
                    message = $"IP not Valid!\n{message}";
                }
                message = $"MQTT Service will be restarted!\n{message}";
            }
            if (back != null)
            {
                modicusWebpages.Default(e);
            }

            mqttRestart = new Thread(() =>
            {
                settingsManager.UpdateSettings();
                commandManager.CmdMqttOnOff.Execute(new Commands.CmdMqttOnOffData { On = false });
                commandManager.CmdMqttOnOff.Execute(new Commands.CmdMqttOnOffData { On = true });
            });
            mqttRestart.Start();

            WebManager.OutPutResponse(e.Context.Response, modicusWebpages.CreateMQTTSettingsPage(message));
        }
    }
}