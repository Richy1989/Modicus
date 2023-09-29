using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using Modicus.Sensor;
using Modicus.Services;
using Modicus.Settings;
using nanoFramework.Hardware.Esp32;

namespace Modicus.Manager
{
    internal class ModicusStartupManager
    {
        public SettingsManager SettingsManager { get; set; }
        public CommandManager CommandManager { get; set; }
        public GlobalSettings GlobalSettings { get; set; }
        public string AsseblyName { get; }

        private readonly GpioController controller;
        private MqttManager mqttManager = null;
        public DateTime startupTime { get; set; }

        public ModicusStartupManager()
        {
            //Close Startup LED to make sure we see the successfull startup at the end
            controller = new GpioController();
            GpioPin pin = controller.OpenPin(Gpio.IO02, PinMode.Output);
            pin.Write(PinValue.Low);

            this.AsseblyName = "modicus";
            this.SettingsManager = new();
            this.SettingsManager.LoadSettings(false);
            this.GlobalSettings = this.SettingsManager.GlobalSettings;

            if (this.GlobalSettings.IsFreshInstall)
            {
                InitializeFrehInstall();
                SettingsManager.UpdateSettings();
            }

            CancellationTokenSource source = new();
            CancellationToken token = source.Token;
            mqttManager = new(this, token);

            /*
            WebManager webManager = new WebManager();
            Thread webThread = new(new ThreadStart(webManager.StartServer));
            webThread.Start();
            */

            BME280Sensor bME280Sensor = new(mqttManager, GlobalSettings, token);
            bME280Sensor.Init();
            Thread bme280Thread = new(new ThreadStart(bME280Sensor.DoMeasurement));
            bme280Thread.Start();

            WiFiManager wifi = null;
            if (GlobalSettings.WifiSettings.ConnectToWifi)
            {
                wifi = new(mqttManager, token);
                wifi.Connect(GlobalSettings.WifiSettings.Ssid, GlobalSettings.WifiSettings.Password);
                Thread wifiThread = new(new ThreadStart(wifi.KeepConnected));
                wifiThread.Start();
            }

            //Wait for successfull wifi connection
            if (wifi != null)
            {
                while (!wifi.IsConnected)
                    Thread.Sleep(100);
            }

            //Starts the NTP Service .. wait 500ms to be sure we have the time
            NTPService ntp = new ();
            Thread.Sleep(1000);

            //Set all Commands for command capable managers
            CommandManager = new CommandManager(SettingsManager, mqttManager);
            CommandManager.AddCommandCapableManager(typeof(MqttManager), mqttManager);
            CommandManager.SetMqttCommands();

            if (GlobalSettings.MqttSettings.ConnectToMqtt)
            {
                mqttManager.InitializeMQTT();
            }

            startupTime = DateTime.UtcNow;
            //Set LED on GPIO Pin 2 ON to show successful startup
            pin.Write(PinValue.High);
        }

        //Initialize the settings for a fresh install. This happens only once
        public void InitializeFrehInstall()
        {
            this.GlobalSettings.MqttSettings.MqttClientID = string.Format("{0}/{1}", AsseblyName, "modicus_sensorrange_office") ;
            //Load the default values only valid for the build environment. Do not make these values Public
#if DEBUG
            Debug.WriteLine("+++++ Write Build Variables to Settings: +++++");
            SettingsManager.GlobalSettings.WifiSettings.ConnectToWifi = true;
            SettingsManager.GlobalSettings.WifiSettings.Ssid = NotPushable.NotPushable.WifiSsid;
            SettingsManager.GlobalSettings.WifiSettings.Password = NotPushable.NotPushable.WifiPassword;
            SettingsManager.GlobalSettings.MqttSettings.ConnectToMqtt = true;
            SettingsManager.GlobalSettings.MqttSettings.MqttUserName = NotPushable.NotPushable.MQTTUserName;
            SettingsManager.GlobalSettings.MqttSettings.MqttPassword = NotPushable.NotPushable.MQTTPassword;
            SettingsManager.GlobalSettings.MqttSettings.MqttHostName = NotPushable.NotPushable.MQTTHostName;
#endif
        }

        //Create a Unique ID based on the MAC address of the controller
        public static string GetUniqueID()
        {
            var ni = NetworkInterface.GetAllNetworkInterfaces();
            if (ni.Length > 0)
            {
                var physicalAddress = ni[0].PhysicalAddress;
                var physicalAddressString = string.Format("{0:X}:{1:X}:{2:X}:{3:X}:{4:X}:{5:X}", physicalAddress[0], physicalAddress[1], physicalAddress[2], physicalAddress[3], physicalAddress[4], physicalAddress[5]);
                Debug.WriteLine($"+++++ Returning MAC: {physicalAddressString} +++++");
                return physicalAddressString;
            }
            else
            {
                var uniqueID = Guid.NewGuid().ToString();
                Debug.WriteLine($"+++++ Returning GUID: {uniqueID} +++++");
                return uniqueID;
            }
        }
    }
}
