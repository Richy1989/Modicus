using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Networking;

namespace GardenLightHyperionConnector.WiFi
{
    public class WiFiManager
    {
        private CancellationToken token;
        public bool IsConnected { get; private set; }

        public WiFiManager(CancellationToken token)
        {
            this.token = token;
        }

        public void Connect(string ssid, string password)
        {
            // Give 60 seconds to the wifi join to happen
            CancellationTokenSource cs = new(60000);

            var success = WifiNetworkHelper.ScanAndConnectDhcp(ssid, password, token: cs.Token);

            //  var success = WifiNetworkHelper.ConnectDhcp(ssid, password, requiresDateTime: true, token: cs.Token);
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
                IsConnected = true;

            Debug.WriteLine($"Successfully Connected to WiFi: {WifiNetworkHelper.Status}");
        }

        public void KeepConnected()
        {
            while (!token.IsCancellationRequested)
            {
                if (WifiNetworkHelper.Status != NetworkHelperStatus.NetworkIsReady)
                {
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
                }
                Thread.Sleep(1000);
            }
        }
    }
}
