using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Threading;
using GardenLightHyperionConnector.Manager;
using Modicus.Commands;
using Modicus.Commands.Interfaces;
using Modicus.Manager.Interfaces;
using Modicus.Sensor.Interfaces;
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

        /// <summary>Initializes a new instance of the <see cref="ModicusWebpageAPI"/> class.</summary>
        /// <param name="settingsManager">The settings manager.</param>
        /// <param name="busDeviceManager">The bus device manager.</param>
        /// <param name="commandManager">The command manager.</param>
        /// <param name="modicusWebpages">The modicus webpages.</param>
        public ModicusWebpageAPI(
            ISettingsManager settingsManager,
            IBusDeviceManager busDeviceManager,
            ICommandManager commandManager,
            ModicusWebpages modicusWebpages)
        {
            this.settingsManager = settingsManager;
            this.commandManager = commandManager;
            this.modicusWebpages = modicusWebpages;
            this.busDeviceManager = busDeviceManager;
        }

        /// <summary>Route for IP Settings. Handels the request to set the new IP Settings and saves everything.</summary>
        /// <param name="e">The <see cref="WebServerEventArgs"/> instance containing the event data.</param>
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

            Thread wifiSetupTask;
            Debug.WriteLine($"IP parameters IP:{ip} Subnetmask:{subnet} Default Gateway {gateway}");

            string message = "";
            if (save != null)
            {
                CmdWifiControlData wifiData = new()
                {
                    IP = ip,
                    DefaultGateway = gateway,
                    NetworkMask = subnet,
                    Ssid = ssid,
                    Password = password,
                    UseDHCP = useDhcp != null && useDhcp == "on",
                    Mode = CmdWifiMode.ConfigureWireless80211
                };

                wifiSetupTask = new Thread(() =>
                {
                    commandManager.CmdWifiControl.Execute(wifiData);
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

        /// <summary>Route fot MQTT Settings. Handles all the config, saves and restarted the MQTT server.</summary>
        /// <param name="e">The <see cref="WebServerEventArgs"/> instance containing the event data.</param>
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

            Thread mqttRestart;
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

        /// <summary>Route for System Settings. Handels the request to set the new system settings.</summary>
        /// <param name="e">The <see cref="WebServerEventArgs"/> instance containing the event data.</param>
        [Route("system_settings")]
        public void SystemSettings(WebServerEventArgs e)
        {
            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);
            var reboot = (string)hashPars["reboot"];
            var back = (string)hashPars["back"];
            var save = (string)hashPars["save"];
            var name = (string)hashPars["device-name"];
            var enableSignal = (string)hashPars["enable-signal"];
            var signalPinString = (string)hashPars["signal-gpio"];

            string message = "";
            Thread systemSettingsTask;

            if (save != null)
            {
                systemSettingsTask = new Thread(() =>
                {
                    settingsManager.GlobalSettings.SystemSettings.InstanceName = name;

                    settingsManager.GlobalSettings.SystemSettings.UseSignalling = enableSignal != null && enableSignal == "on";
                    if (settingsManager.GlobalSettings.SystemSettings.UseSignalling)
                        message = $"Signal GPIO is activated - Please reboot device.\n{message}";
                    else
                        message = $"Signal GPIO is deactivated - Please reboot device.\n{message}";

                    if (int.TryParse(signalPinString, out var signalPin))
                    {
                        settingsManager.GlobalSettings.SystemSettings.SignalGpioPin = signalPin;
                        message = $"New Signal GPIO Pin: {signalPin} - Please reboot device.\n{message}";
                    }

                    settingsManager.UpdateSettings();
                });
                systemSettingsTask.Start();
                message = $"New Device Name: {name}\n{message}";
            }

            if (reboot != null)
            {
                systemSettingsTask = new Thread(() =>
                {
                    commandManager.CmdSystemReboot.Execute(new CmdRebootControllerData { Delay = 2 });
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
                if (inter.FullName == typeof(II2cSensor).FullName)
                {
                    WebManager.OutPutResponse(e.Context.Response, modicusWebpages.CreateI2CSettingsSite("Configure Away!", item));
                    return;
                }
            }
            string message = "Select your desired sensor ...";
            WebManager.OutPutResponse(e.Context.Response, modicusWebpages.CreateSensorSelectionSite(message));
        }

        [Route("create_i2c_sensor")]
        public void CreateI2cSensor(WebServerEventArgs e)
        {
            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);

            string message = "Error in sensor creation:\n";
            var item = (string)hashPars["sensor-type"];
            var back = (string)hashPars["back"];
            var sensor = busDeviceManager.SupportedSensors[item];

            bool isOk = true;

            if (back != null)
            {
                WebManager.OutPutResponse(e.Context.Response, modicusWebpages.CreateSensorSelectionSite(""));
                return;
            }

            if (!int.TryParse((string)hashPars["i2c-bus-speed"], out var speed))
            {
                message = $"Speed value not valid.\n{message}";
                isOk = false;
            }
            if (!int.TryParse((string)hashPars["measurement-interval"], out var minterval))
            {
                message = $"Measurement Interval value not valid.\n{message}";
                isOk = false;
            }

            if (!int.TryParse((string)hashPars["scl"], out var scl))
            {
                message = $"SCL Pin value not valid.\n{message}";
                isOk = false;
            }

            if (!int.TryParse((string)hashPars["sda"], out var sda))
            {
                message = $"SDA Pin value not valid.\n{message}";
                isOk = false;
            }

            if (!int.TryParse((string)hashPars["device-address"], out var address))
            {
                message = $"Device Address value not valid.\n{message}";
                isOk = false;
            }

            if (!int.TryParse((string)hashPars["bus-id"], out var busid))
            {
                message = $"Bud ID value not valid.\n{message}";
                isOk = false;
            }

            if (string.IsNullOrEmpty((string)hashPars["name"]))
            {
                message = $"Name value not valid.\n{message}";
                isOk = false;
            }

            string name = (string)hashPars["name"];
            string category = (string)hashPars["category"];

            if (isOk)
            {
                CmdCreateI2CSensorData data = new()
                {
                    I2cBusSpeed = speed,
                    MeasurementInterval = minterval,
                    SclPin = scl,
                    SdaPin = sda,
                    DeviceAddress = address,
                    Name = name,
                    BusID = busid,
                    SensorType = item,
                    MeasurementCategory = category
                    
                };

                Thread createTask = new(() =>
                {
                    commandManager.CmdCreateI2CSensor.Execute(data);
                });
                createTask.Start();

                WebManager.OutPutResponse(e.Context.Response, modicusWebpages.CreateI2CSettingsSite($"Sensor: {name} crated!", item));
                return;
            }
            WebManager.OutPutResponse(e.Context.Response, modicusWebpages.CreateI2CSettingsSite("Configure Away!", item));
        }

        [Route("edit_sensor")]
        public void EditSensor(WebServerEventArgs e)
        {
            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);
            var start = (string)hashPars["start"];
            var stop = (string)hashPars["stop"];
            var delete = (string)hashPars["delete"];
            var back = (string)hashPars["back"];
            var sensorString = (string)hashPars["selected_sensor"];

            ISensor sensor = busDeviceManager.GetSensorFromName(sensorString);

            string message = string.Empty;
            if (sensor != null)
            {
                if (start != null)
                {
                    busDeviceManager.StartSensor(sensor);
                    message = $"Sensor: {sensor.Name} Started!";
                }
                if (stop != null)
                {
                    busDeviceManager.StopSensor(sensor);
                    message = $"Sensor: {sensor.Name} Stopped!";
                }
                if (delete != null)
                {
                    busDeviceManager.DeleteSensor(sensor);
                    message = $"Sensor: {sensor.Name} Deleted!";
                }
            }
            else if (back != null)
            {
                modicusWebpages.Default(e);
                return;
            }

            WebManager.OutPutResponse(e.Context.Response, modicusWebpages.CreateSensorSelectionSite(message));
        }
    }
}