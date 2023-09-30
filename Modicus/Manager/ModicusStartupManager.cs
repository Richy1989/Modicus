﻿using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using Modicus.Commands.Interfaces;
using Modicus.Interfaces;
using Modicus.MQTT.Interfaces;
using Modicus.Sensor;
using Modicus.Services;
using Modicus.Settings;
using nanoFramework.Hardware.Esp32;

namespace Modicus.Manager
{
    internal class ModicusStartupManager
    {
        public ISettingsManager SettingsManager { get; set; }
        public ICommandManager CommandManager { get; set; }
        public IWebManager WebManager { get; set; }
        public GlobalSettings GlobalSettings { get; set; }
        public string AsseblyName { get; }

        private IServiceProvider serviceProvider;

        private readonly GpioController controller;
        private IMqttManager mqttManager = null;

        public ModicusStartupManager(IServiceProvider serviceProvider, ITokenManager tokenManager, ISettingsManager settingsManager, IWebManager webManager, IMqttManager mqttManager, ICommandManager commandManager)
        {
            //Close Startup LED to make sure we see the successfull startup at the end
            controller = new GpioController();
            GpioPin pin = controller.OpenPin(Gpio.IO02, PinMode.Output);
            pin.Write(PinValue.Low);

            AsseblyName = "modicus";
            SettingsManager = settingsManager;
            GlobalSettings = SettingsManager.GlobalSettings;
            CancellationToken token = tokenManager.Token;

            if (GlobalSettings.IsFreshInstall)
            {
                InitializeFrehInstall();
                SettingsManager.UpdateSettings();
            }

            this.serviceProvider = serviceProvider;
            this.mqttManager = mqttManager;

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

            //Starts the NTP Service .. wait 500ms to be sure we have the time
            NTPService ntp = new();
            Thread.Sleep(1000);

            //Set all Commands for command capable managers
            CommandManager = commandManager;// new CommandManager(settingsManager, mqttManager);
            CommandManager.AddCommandCapableManager(typeof(MqttManager), mqttManager);
            CommandManager.SetMqttCommands();

            ///Start MQTT Service if needed
            if (GlobalSettings.MqttSettings.ConnectToMqtt)
            {
                Thread mqttStartTask = new(new ThreadStart(() =>
                {
                    CommandManager.CmdMqttOnOff.Execute(new Commands.CmdMqttOnOffData { On = true });
                }));

                mqttStartTask.Start();
            }

            WebManager = webManager;
            Thread webTask = new(new ThreadStart(WebManager.StartWebManager));
            webTask.Start();

            settingsManager.GlobalSettings.StartupTime = DateTime.UtcNow;
            //Set LED on GPIO Pin 2 ON to show successful startup
            pin.Write(PinValue.High);
        }

        //Initialize the settings for a fresh install. This happens only once
        public void InitializeFrehInstall()
        {
            this.GlobalSettings.MqttSettings.MqttClientID = string.Format("{0}/{1}", AsseblyName, $"modicus_sensorrange_{GetUniqueID()}");
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