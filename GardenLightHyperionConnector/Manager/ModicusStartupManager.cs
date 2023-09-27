using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using GardenLightHyperionConnector.NotPushable;
using GardenLightHyperionConnector.Sensor;
using GardenLightHyperionConnector.Services;
using GardenLightHyperionConnector.Settings;
using GardenLightHyperionConnector.WiFi;
using LuminInside.MQTT;
using nanoFramework.Hardware.Esp32;

namespace NFApp1.Manager
{
    public class ModicusStartupManager

    {
        public SettingsManager SettingsManager { get; set; }
        public GlobalSettings GlobalSettings { get; set; }
        public string AsseblyName { get; }

        private readonly GpioController controller;
        private MqttManager mqttManager = null;

        public ModicusStartupManager()
        {
            //Close Startup LED to make sure we see the successfull startup at the end
            controller = new GpioController();
            GpioPin pin = controller.OpenPin(Gpio.IO02, PinMode.Output);
            pin.Write(PinValue.Low);

            this.AsseblyName = "modicus";
            this.SettingsManager = new();
            this.SettingsManager.LoadSettings(true);
            this.GlobalSettings = this.SettingsManager.GlobalSettings;
            this.GlobalSettings.MqttSettings.MqttClientID = "modicus_sensorrange_livingroom";
            //Load the default values only valid for the build environment. Do not make these values Public
#if DEBUG
            Debug.WriteLine("+++++ Write Build Variables to Settings: +++++");
            SettingsManager.GlobalSettings.WifiSettings.ConnectToWifi = true;
            SettingsManager.GlobalSettings.WifiSettings.Ssid = NotPushable.WifiSsid;
            SettingsManager.GlobalSettings.WifiSettings.Password = NotPushable.WifiPassword;
            SettingsManager.GlobalSettings.MqttSettings.ConnectToMqtt = true;
            SettingsManager.GlobalSettings.MqttSettings.MqttUserName = NotPushable.MQTTUserName;
            SettingsManager.GlobalSettings.MqttSettings.MqttPassword = NotPushable.MQTTPassword;
            SettingsManager.GlobalSettings.MqttSettings.MqttHostName = NotPushable.MQTTHostName;
#endif

            CancellationTokenSource source = new();
            CancellationToken token = source.Token;
            mqttManager = new(GlobalSettings, token);

            /*
            WebManager webManager = new WebManager();
            Thread webThread = new(new ThreadStart(webManager.StartServer));
            webThread.Start();
            */

            BME280Sensor bME280Sensor = new BME280Sensor(mqttManager, GlobalSettings, token);
            bME280Sensor.Init();
            Thread bme280Thread = new(new ThreadStart(bME280Sensor.DoMeasurement));
            bme280Thread.Start();

            WiFiManager wifi = null;
            if (GlobalSettings.WifiSettings.ConnectToWifi)
            {
                wifi = new(token);
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
            

            if (GlobalSettings.MqttSettings.ConnectToMqtt)
            {
                mqttManager.Connect(
                    GlobalSettings.MqttSettings.MqttHostName,
                    string.Format("{0}/{1}", AsseblyName, GlobalSettings.MqttSettings.MqttClientID),
                    GlobalSettings.MqttSettings.MqttUserName,
                    GlobalSettings.MqttSettings.MqttPassword);
                mqttManager.InitializeMQTT();

               Thread mqttThread = new(new ThreadStart(mqttManager.StartSending));
               mqttThread.Start();
            }

            //Set LED on GPIO Pin 2 ON to show successful startup
            pin.Write(PinValue.High);
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
