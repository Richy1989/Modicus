using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using nanoFramework.Networking;
using Modicus.MQTT.Interfaces;

namespace Modicus.Manager
{
    internal class WiFiManager
    {
        private CancellationToken token;
        private readonly IPublishMqtt publishMqtt;
        private TimeSpan downTime;
        public bool IsConnected { get; private set; }

        public WiFiManager(IMqttManager publishMqtt, CancellationToken token)
        {
            this.publishMqtt = (IPublishMqtt)publishMqtt;
            this.token = token;
            var ni = NetworkInterface.GetAllNetworkInterfaces();
            if (ni.Length > 0)
            {
                var physicalAddress = ni[0].PhysicalAddress;
                this.publishMqtt.State.WiFi.BSSId = BitConverter.ToString(physicalAddress);
            }
        }

        public void Connect(string ssid, string password)
        {
            // Give 60 seconds to the wifi join to happen
            CancellationTokenSource cs = new(60000);

            var success = WifiNetworkHelper.ScanAndConnectDhcp(ssid, password, token: cs.Token);

            IsConnected = false;
            if (!success)
            {
                // Something went wrong, you can get details with the ConnectionError property:
                Debug.WriteLine($"Can't connect to the network, error: {WifiNetworkHelper.Status}");
                if (WifiNetworkHelper.HelperException != null)
                {
                    Debug.WriteLine($"ex: {WifiNetworkHelper.HelperException}");
                }
            }
            else
            {
                IsConnected = true;
                publishMqtt.State.WiFi.SSId = ssid;
            }

            Debug.WriteLine($"Successfully Connected to WiFi: {WifiNetworkHelper.Status}");
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
