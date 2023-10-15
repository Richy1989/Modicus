using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using Modicus.Commands;
using Modicus.Commands.Interfaces;
using Modicus.Helpers;
using Modicus.Helpers.Interfaces;
using Modicus.Manager.Interfaces;
using Modicus.MQTT.Interfaces;
using Modicus.Settings;
using Modicus.Wifi.Interfaces;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ModicusStartupManager"/> class.
        /// Creates the Modicus Startup Manager, handles the whole application.
        /// </summary>
        /// <param name="tokenManager">The token manager.</param>
        /// <param name="settingsManager">The settings manager.</param>
        /// <param name="wifiManager">The wifi manager.</param>
        /// <param name="webManager">The web manager.</param>
        /// <param name="mqttManager">The MQTT manager.</param>
        /// <param name="busManager">The bus manager.</param>
        /// <param name="commandManager">The command manager.</param>
        public ModicusStartupManager(ITokenManager tokenManager,
            ISettingsManager settingsManager,
            IWiFiManager wifiManager,
            IWebManager webManager,
            IMqttManager mqttManager,
            IBusDeviceManager busManager,
            ICommandManager commandManager,
            ISignalService signalService)
        {
            signalService.SignalOff();

            AsseblyName = "modicus";
            SettingsManager = settingsManager;
            GlobalSettings = SettingsManager.GlobalSettings;
            CancellationToken token = tokenManager.Token;

            if (GlobalSettings.IsFreshInstall)
            {
                InitializeFrehInstall();
                SettingsManager.UpdateSettings();
            }

            if (GlobalSettings.WifiSettings.ConnectToWifi)
            {
                commandManager.CmdWifiControl.Execute(new CmdWifiControlData { Mode = CmdWifiMode.StartOnly });
            }

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
                    Debug.WriteLine($"We have a valid date: {DateTime.UtcNow}");
                    Diagnostics.PrintMemory("Modicus:");
                    Thread.Sleep(10000);
                }
            }));

            diagTask.Start();
#endif
            //Set LED on GPIO Pin 2 ON, to show successful startup
            signalService.SignalOn();
        }

        //Initialize the settings for a fresh install. This happens only once
        public void InitializeFrehInstall()
        {
            this.GlobalSettings.MqttSettings.MqttClientID = string.Format("{0}/{1}", AsseblyName, $"modicus_sensorrange_{GetUniqueID()}");
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