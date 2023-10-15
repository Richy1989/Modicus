using System;
using System.Diagnostics;
using System.Net;
using Modicus.Manager.Interfaces;
using Modicus.Wifi.Interfaces;
using Modicus.WiFi;
using nanoFramework.Json;

namespace Modicus.Commands
{
    //With this command the IP Settings can be set
    internal class CmdWifiControl : BaseCommand
    {
        private readonly IWiFiManager wiFiManager;
        private readonly ISettingsManager settingsManager;
                      
        public CmdWifiControl(ISettingsManager settingsManager, IWiFiManager wiFiManager)
        {
            this.wiFiManager = wiFiManager;
            this.settingsManager = settingsManager;
            Topic = settingsManager.CommandSettings.WifiControl;
        }

        public bool Execute(CmdWifiControlData content)
        {
            if (content == null)
            {
                Debug.WriteLine($"Command: Wifi Control -> No Payload!");
                return false;
            }

            var wifiSettings = settingsManager.GlobalSettings.WifiSettings;

            if (content.Mode == CmdWifiMode.ConfigureAccessPoint)
            {
                WirelessAP.Setup();
                wifiSettings.StartInAPMode = true;
            }

            if (content.Mode == CmdWifiMode.ConfigureWireless80211)
            {
                if (!content.UseDHCP)
                {
                    try
                    {
                        IPAddress.Parse(content.IP);
                        wifiSettings.IP = content.IP;

                        IPAddress.Parse(content.DefaultGateway);
                        wifiSettings.DefaultGateway = content.DefaultGateway;

                        IPAddress.Parse(content.NetworkMask);
                        wifiSettings.NetworkMask = content.NetworkMask;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error in Wifi control command: {ex.Message}");
                        return false;
                    }
                }

                wifiSettings.Ssid = content.Ssid;
                wifiSettings.Password = content.Password;
                wifiSettings.StartInAPMode = false;

                settingsManager.UpdateSettings();
                Wireless80211.Configure(wifiSettings);
                WirelessAP.Disable();
            }

            wiFiManager.Start();

            return true;
        }

        //Execute the command
        public new void Execute(string content)
        {
            base.mreExecute.WaitOne();

            CmdWifiControlData data = (CmdWifiControlData)JsonConvert.DeserializeObject(content, typeof(CmdWifiControlData));
            if (Execute(data))
                base.Execute(content);

            base.mreExecute.Set();
        }
    }

    //This class contraints the content of the message which will be sent via JSON
    internal class CmdWifiControlData
    {
        public string Ssid { get; set; }
        public string Password { get; set; }
        public string IP { get; set; }
        public string NetworkMask { get; set; }
        public string DefaultGateway { get; set; }
        public bool UseDHCP { get; set; }
        public CmdWifiMode Mode { get; set; }
    }

    internal enum CmdWifiMode
    {
        StartOnly,
        ConfigureAccessPoint,
        ConfigureWireless80211
    }
}