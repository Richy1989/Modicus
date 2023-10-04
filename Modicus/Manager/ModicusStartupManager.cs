﻿using System;
using System.Collections;
using System.Device.Gpio;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using Iot.Device.Bmxx80;
using Modicus.Commands.Interfaces;
using Modicus.Helpers;
using Modicus.Manager.Interfaces;
using Modicus.MQTT.Interfaces;
using Modicus.Sensor;
using Modicus.Sensor.Interfaces;
using Modicus.Settings;
using Modicus.Wifi.Interfaces;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Json;
using GC = nanoFramework.Runtime.Native.GC;

namespace Modicus.Manager
{
    internal class ModicusStartupManager
    {
        public ISettingsManager SettingsManager { get; set; }
        public ICommandManager CommandManager { get; set; }
        public IWebManager WebManager { get; set; }
        public GlobalSettings GlobalSettings { get; set; }
        public string AsseblyName { get; }

        public static GpioPin pin;

        private readonly GpioController controller;

        public ModicusStartupManager(ITokenManager tokenManager,
            ISettingsManager settingsManager,
            IWiFiManager wifiManager,
            IWebManager webManager,
            IMqttManager mqttManager,
            IBusDeviceManager busManager,
            ICommandManager commandManager)
        {
            //Close Startup LED to make sure we see the successfull startup at the end
            controller = new GpioController();
            pin = controller.OpenPin(Gpio.IO02, PinMode.Output);
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

            //ToDo: delete this later, makes dubugging better
            if (busManager.GetSensor("bme280#1") == null)
            {
                II2cSensor bme280 = new BME280Sensor();
                bme280.BusID = 1;
                bme280.MeasurementInterval = 1000;
                bme280.DeviceAddress = Bme280.SecondaryI2cAddress;
                bme280.I2cBusSpeed = System.Device.I2c.I2cBusSpeed.StandardMode;
                bme280.Name = "bme280#1";
                bme280.SdaPin = Gpio.IO21;
                bme280.SclPin = Gpio.IO22;

                busManager.AddSensor(bme280);
                busManager.StartSensor(bme280);
            }

            if (GlobalSettings.WifiSettings.ConnectToWifi)
            {
                wifiManager.Start();
            }

            ////if (!wifiManager.ISoftAP)
            ////{
            ////    //Starts the NTP Service .. wait 500ms to be sure we have the time
            ////    NTPService ntp = new();
            ////    Thread.Sleep(1000);
            ////}

            //Set all Commands for command capable managers
            CommandManager = commandManager;
            CommandManager.AddCommandCapableManager(typeof(MqttManager), mqttManager);
            CommandManager.SetMqttCommands();

            ///Start MQTT Service if needed
            if (GlobalSettings.MqttSettings.ConnectToMqtt && !wifiManager.ISoftAP)
            {
                Thread mqttStartTask = new(new ThreadStart(() =>
                {
                    CommandManager.CmdMqttOnOff.Execute(new Commands.CmdMqttOnOffData { On = true });
                }));

                mqttStartTask.Start();
            }

            if (wifiManager.IsConnected)
            {
                //Start the web manager
                webManager.StartWebManager();
            }

            //Set startup time
            settingsManager.GlobalSettings.StartupTime = DateTime.UtcNow;

            //Run GC once when everything is set up
            // ToDo: check if we need that.
            GC.Run(true);
#if DEBUG
            Thread diagTask = new(new ThreadStart(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    Diagnostics.PrintMemory("Modicus:");
                    Thread.Sleep(10000);
                }
            }));

            diagTask.Start();
#endif
            //Set LED on GPIO Pin 2 ON, to show successful startup
            pin.Write(PinValue.High);
        }

        //Initialize the settings for a fresh install. This happens only once
        public void InitializeFrehInstall()
        {
            this.GlobalSettings.MqttSettings.MqttClientID = string.Format("{0}/{1}", AsseblyName, $"modicus_sensorrange_{GetUniqueID()}");
            //Load the default values only valid for the build environment. Do not make these values Public
#if DEBUG
            ////Debug.WriteLine("+++++ Write Build Variables to Settings: +++++");
            ////SettingsManager.GlobalSettings.WifiSettings.ConnectToWifi = true;
            ////SettingsManager.GlobalSettings.WifiSettings.Ssid = NotPushable.NotPushable.WifiSsid;
            ////SettingsManager.GlobalSettings.WifiSettings.Password = NotPushable.NotPushable.WifiPassword;
            ////SettingsManager.GlobalSettings.MqttSettings.ConnectToMqtt = true;
            ////SettingsManager.GlobalSettings.MqttSettings.MqttUserName = NotPushable.NotPushable.MQTTUserName;
            ////SettingsManager.GlobalSettings.MqttSettings.MqttPassword = NotPushable.NotPushable.MQTTPassword;
            ////SettingsManager.GlobalSettings.MqttSettings.MqttHostName = NotPushable.NotPushable.MQTTHostName;
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

        //Create a Unique ID based on the MAC address of the controller
        public static string GetUniqueIDPlainString()
        {
            var ni = NetworkInterface.GetAllNetworkInterfaces();
            if (ni.Length > 0)
            {
                var physicalAddress = ni[0].PhysicalAddress;
                var physicalAddressString = string.Format("{0:X}{1:X}{2:X}{3:X}{4:X}{5:X}", physicalAddress[0], physicalAddress[1], physicalAddress[2], physicalAddress[3], physicalAddress[4], physicalAddress[5]);
                Debug.WriteLine($"+++++ Returning uniq string: {physicalAddressString} +++++");
                return physicalAddressString;
            }
            throw new Exception("Cannot create unique string id.");
        }
    }
}