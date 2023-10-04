using System;
using System.Device.Wifi;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using Iot.Device.DhcpServer;
using Modicus.Helpers;
using Modicus.Helpers.Interfaces;
using Modicus.Manager.Interfaces;
using Modicus.MQTT.Interfaces;
using Modicus.Wifi.Interfaces;
using Modicus.WiFi;
using nanoFramework.Networking;
using nanoFramework.Runtime.Native;

namespace Modicus.Manager
{
    internal class WiFiManager : IWiFiManager
    {
        private CancellationToken token;
        private readonly IPublishMqtt publishMqtt;
        private TimeSpan downTime;
        private ISettingsManager settingsManager;
        private ISignalService signalService;
        public bool IsConnected { get; private set; }
        public bool ISoftAP { get; private set; }

        public WiFiManager(IMqttManager publishMqtt, ISettingsManager settingsManager, ISignalService signalService)
        {
            this.signalService = signalService;
            this.publishMqtt = (IPublishMqtt)publishMqtt;
            this.settingsManager = settingsManager;
            var ni = NetworkInterface.GetAllNetworkInterfaces();
            if (ni.Length > 0)
            {
                var physicalAddress = ni[0].PhysicalAddress;
                this.publishMqtt.State.WiFi.BSSId = BitConverter.ToString(physicalAddress);
            }
        }

        public void Start()
        {
            var wifiSettings = settingsManager.GlobalSettings.WifiSettings;
            // If Wireless station is not enabled then start Soft AP to allow Wireless configuration
            // or Button pressed
            if (!Wireless80211.IsEnabled() || wifiSettings.StartInAPMode)
            {
                ISoftAP = true;
                Wireless80211.Disable();
                if (WirelessAP.Setup() == false)
                {
                    // Reboot device to Activate Access Point on restart
                    Debug.WriteLine($"Setup Soft AP, Rebooting device");
                    IsConnected = false;
                    return;
                    //Power.RebootDevice();
                }

                var dhcpserver = new DhcpServer
                {
                    CaptivePortalUrl = $"http://{WirelessAP.SoftApIP}"
                };

                var dhcpInitResult = dhcpserver.Start(IPAddress.Parse(WirelessAP.SoftApIP), new IPAddress(new byte[] { 255, 255, 255, 0 }));

                if (!dhcpInitResult)
                {
                    Debug.WriteLine($"Error initializing DHCP server.");
                    signalService.SignalError();
                }

                IsConnected = true;
                Debug.WriteLine($"Running Soft AP, waiting for client to connect");
                Debug.WriteLine($"Soft AP IP address :{WirelessAP.GetIP()}");
            }
            else
            {
                ISoftAP = false;
                Debug.WriteLine($"Running in normal mode, connecting to Access point");
                bool success;

                if (wifiSettings.UseDHCP)
                    success = WifiNetworkHelper.ScanAndConnectDhcp(wifiSettings.Ssid, wifiSettings.Password);
                else
                {
                    IPConfiguration iPConfiguration = new IPConfiguration(wifiSettings.IP, wifiSettings.NetworkMask, wifiSettings.DefaultGateway);
                    success = WifiNetworkHelper.ConnectFixAddress(wifiSettings.Ssid, wifiSettings.Password, iPConfiguration, System.Device.Wifi.WifiReconnectionKind.Automatic, false, 0, token: new CancellationTokenSource(10000).Token);
                }

                IsConnected = success;
                if (success)
                {
                    Debug.WriteLine($"Connection is {success}");
                    Debug.WriteLine($"We have a valid date: {DateTime.UtcNow}");
                }
                else
                {
                    Debug.WriteLine($"Something wrong happened, can't connect at all");
                    signalService.SignalError();
                }
            }
        }

        public void KeepConnected()
        {
            while (!token.IsCancellationRequested)
            {
                if (WifiNetworkHelper.Status != NetworkHelperStatus.NetworkIsReady)
                {
                    downTime = downTime + TimeSpan.FromSeconds(1);
                    var success = WifiNetworkHelper.Reconnect();

                    if (!success)
                    {
                        // Something went wrong, you can get details with the ConnectionError property:
                        Debug.WriteLine($"Can't connect to the network, error: {WifiNetworkHelper.Status}");
                        if (WifiNetworkHelper.HelperException != null)
                        {
                            Debug.WriteLine($"ex: {WifiNetworkHelper.HelperException}");
                        }
                    }

                    publishMqtt.State.WiFi.DownTime = downTime;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}