using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Threading;
using GardenLightHyperionConnector.Manager;
using Modicus.Commands.Interfaces;
using Modicus.Manager;
using Modicus.Manager.Interfaces;
using Modicus.Sensor.Interfaces;
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
        private readonly IBusDeviceManager busDeviceManager;
        private Thread mqttRestart;
        private Thread wifiSetupTask;
        private Thread systemSettingsTask;

        /// <summary>
        /// Creates a new ModicusWebpageAPI instance.
        /// </summary>
        public ModicusWebpageAPI(ISettingsManager settingsManager, IBusDeviceManager busDeviceManager, ICommandManager commandManager, ModicusWebpages modicusWebpages)
        {
            this.settingsManager = settingsManager;
            this.commandManager = commandManager;
            this.modicusWebpages = modicusWebpages;
            this.busDeviceManager = busDeviceManager;
        }

        /// <summary>
        /// Route for IP Settings. Handels the request to set the new IP Settings and saves everything
        /// </summary>
        /// <param name="e"></param>

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
            var save = (string)hashPars["save"];
            var back = (string)hashPars["back"];

            Debug.WriteLine($"IP parameters IP:{ip} Subnetmask:{subnet} Default Gateway {gateway}");

            string message = "";
            if (save != null)
            {
                var wifiSettings = settingsManager.GlobalSettings.WifiSettings;
                wifiSettings.UseDHCP = useDhcp != null && useDhcp == "on";

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
                    WirelessAP.Disable();
                });
                wifiSetupTask.Start();

                message = $"Reboot Controller!\n{message}";
            }
            if (back != null)
            {
                modicusWebpages.Default(e);
                return;
            }

            WebManager.OutPutResponse(e.Context.Response, modicusWebpages.CreateIPSettingsPage(message));
        }

        /// <summary>
        /// Route fot MQTT Settings. Handles all the config, saves and restarted the MQTT server
        /// </summary>
        /// <param name="e"></param>
        [Route("mqtt_settings")]
        public void MqttSettings(WebServerEventArgs e)
        {
            var mqttSettings = settingsManager.GlobalSettings.MqttSettings;
            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);

            var enableMqtt = (string)hashPars["enable-mqtt"];
            var clientID = (string)hashPars["mqtt-clientid"];
            var sendInterval = (string)hashPars["mqtt-send-interval"];
            var user = (string)hashPars["mqtt-user"];
            var password = (string)hashPars["mqtt-password"];
            var ip = (string)hashPars["mqtt-server-ip"];
            var port = (string)hashPars["mqtt-server-port"];
            var save = (string)hashPars["save"];
            var back = (string)hashPars["back"];

            string message = "";
            if (save != null)
            {
                mqttSettings.ConnectToMqtt = enableMqtt != null && enableMqtt == "on";

                mqttSettings.MqttPort = int.TryParse(port, out var resultPort) ? resultPort : mqttSettings.MqttPort;
                mqttSettings.MqttClientID = clientID;
                mqttSettings.MqttUserName = user;
                mqttSettings.MqttPassword = password;

                if (int.TryParse(sendInterval, out var result))
                {
                    if (result >= 1)
                        mqttSettings.SendInterval = TimeSpan.FromSeconds(result);
                }
                else
                    message = $"Send Interval not Valid!\n{message}";

                try
                {
                    IPAddress.Parse(ip);
                    mqttSettings.MqttHostName = ip;
                }
                catch
                {
                    message = $"IP not Valid!\n{message}";
                }
                message = $"MQTT Service will be restarted!\n{message}";

                mqttRestart = new Thread(() =>
                {
                    settingsManager.UpdateSettings();
                    commandManager.CmdMqttOnOff.Execute(new Commands.CmdMqttOnOffData { On = false });

                    if (mqttSettings.ConnectToMqtt)
                        commandManager.CmdMqttOnOff.Execute(new Commands.CmdMqttOnOffData { On = true });
                });
                mqttRestart.Start();
            }
            if (back != null)
            {
                modicusWebpages.Default(e);
                return;
            }

            WebManager.OutPutResponse(e.Context.Response, modicusWebpages.CreateMQTTSettingsPage(message));
        }

        /// <summary>
        /// Route for System Settings. Handels the request to set the new system settings
        /// </summary>
        /// <param name="e"></param>

        [Route("system_settings")]
        public void SystemSettings(WebServerEventArgs e)
        {
            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);
            var reboot = (string)hashPars["reboot"];
            var back = (string)hashPars["back"];
            var save = (string)hashPars["save"];
            var name = (string)hashPars["device-name"];

            string message = "";

            if (save != null)
            {
                systemSettingsTask = new Thread(() =>
                {
                    settingsManager.GlobalSettings.InstanceName = name;
                    settingsManager.UpdateSettings();
                });
                systemSettingsTask.Start();
                message = $"New Device Name: {name}\n{message}";

            }

            if (reboot != null)
            {
                systemSettingsTask = new Thread(() =>
                {
                    commandManager.CmdSystemReboot.Execute(new Commands.CmdRebootControllerData { Delay = 2 });
                });
                systemSettingsTask.Start();

                message = $"Controller is rebooting now!\n{message}";
            }
            if (back != null)
            {
                modicusWebpages.Default(e);
                return;
            }

            WebManager.OutPutResponse(e.Context.Response, modicusWebpages.CreateSystemSettingsPage(message));
        }

        [Route("configure_sensor")]
        public void ConfigureSensor(WebServerEventArgs e)
        {
            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);
            var item = (string)hashPars["item"];
            var interfaces = ((Type)busDeviceManager.SupportedSensors[item]).GetInterfaces();

            foreach (Type inter in interfaces)
            {
                switch (inter)
                {
                    case II2cSensor:
                        
                        return;
                    default:
                        break;

                }
            }

            string message = "Select your desired sensor ...";
            WebManager.OutPutResponse(e.Context.Response, modicusWebpages.CreateSensorSelectionSite(message));
        }
    }
}