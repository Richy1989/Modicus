using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Threading;
using GardenLightHyperionConnector.Manager;
using Modicus.Commands.Interfaces;
using Modicus.Interfaces;
using Modicus.WiFi;
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

        private Thread wifiSetupTask;

        [Route("ip_settings")]
        public void IPSettings(WebServerEventArgs e)
        {
            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);
            var ssid = (string)hashPars["ssid"];
            var password = (string)hashPars["password"];
            var ip = (string)hashPars["ip-address"];
            var subnet = (string)hashPars["subnetmask"];
            var gateway = (string)hashPars["default-gateway"];
            var useDhcp = (string)hashPars["use-dhcp"];

            Debug.WriteLine($"IP parameters IP:{ip} Subnetmask:{subnet} Default Gateway {gateway}");

            var wifiSettings = settingsManager.GlobalSettings.WifiSettings;

            wifiSettings.UseDHCP = useDhcp != null && useDhcp == "on";

            string message = "";
            if (!wifiSettings.UseDHCP)
            {
                string errorMessage = "IP";
                try
                {
                    IPAddress.Parse(ip);
                    wifiSettings.IP = ip;

                    errorMessage = "Default Gateway";
                    IPAddress.Parse(gateway);
                    wifiSettings.DefaultGateway = gateway;

                    errorMessage = "Subnetmask";
                    IPAddress.Parse(subnet);
                    wifiSettings.NetworkMask = subnet;
                }
                catch
                {
                    message = $"{errorMessage} not Valid!\n{message}";
                    message = $"DHCP use not activated!\n{message}";
                    wifiSettings.UseDHCP = false;
                }
            }

            wifiSettings.Ssid = ssid;
            wifiSettings.Password = password;
            wifiSettings.StartInAPMode = false;

            wifiSetupTask = new Thread(() =>
            {
                settingsManager.UpdateSettings();
                Wireless80211.Configure(wifiSettings);
            });
            wifiSetupTask.Start();

            message = $"Reboot Controller!\n{message}";

            WebManager.OutPutResponse(e.Context.Response, modicusWebpages.CreateIPSettingsPage(message));
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