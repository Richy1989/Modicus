//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using Modicus.Settings;
using nanoFramework.Networking;

namespace Modicus.WiFi
{
    internal class Wireless80211
    {
        /// <summary>Determines whether Wireless80211 is enabled.</summary>
        /// <returns><c>true</c> if this instance is enabled; otherwise, <c>false</c>.</returns>
        public static bool IsEnabled()
        {
            Wireless80211Configuration wconf = GetConfiguration();
            return !string.IsNullOrEmpty(wconf.Ssid);
        }

        /// <summary>Disable the Wireless station interface.</summary>
        public static void Disable()
        {
            Wireless80211Configuration wconf = GetConfiguration();
            wconf.Options = Wireless80211Configuration.ConfigurationOptions.None;
            wconf.SaveConfiguration();
        }

        /// <summary>Configures the specified wifi settings.</summary>
        /// <param name="wifiSettings">The wifi settings.</param>
        public static void Configure(WifiSettings wifiSettings)
        {
            // And we have to force connect once here even for a short time
            bool success;
            if (wifiSettings.UseDHCP)
                success = WifiNetworkHelper.ConnectDhcp(wifiSettings.Ssid, wifiSettings.Password, token: new CancellationTokenSource(10000).Token);
            else
            {
                IPConfiguration iPConfiguration = new(wifiSettings.IP, wifiSettings.NetworkMask, wifiSettings.DefaultGateway);
                success = WifiNetworkHelper.ConnectFixAddress(wifiSettings.Ssid, wifiSettings.Password, iPConfiguration, System.Device.Wifi.WifiReconnectionKind.Automatic, false, 0, token: new CancellationTokenSource(10000).Token);
            }

            Debug.WriteLine($"Connection is {success}");

            Wireless80211Configuration wconf = GetConfiguration();
            wconf.Options = Wireless80211Configuration.ConfigurationOptions.AutoConnect | Wireless80211Configuration.ConfigurationOptions.Enable;
            wconf.SaveConfiguration();
            return;
        }

        /// <summary>Get the Wireless station configuration.</summary>
        /// <returns>Wireless80211Configuration object</returns>
        public static Wireless80211Configuration GetConfiguration()
        {
            NetworkInterface ni = GetInterface();
            return Wireless80211Configuration.GetAllWireless80211Configurations()[ni.SpecificConfigId];
        }

        /// <summary>Gets the network interface.</summary>
        /// <returns>NetworkInterface</returns>
        public static NetworkInterface GetInterface()
        {
            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();

            // Find WirelessAP interface
            foreach (NetworkInterface ni in Interfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    return ni;
                }
            }
            return null;
        }

        /// <summary>Returns the IP address.</summary>
        /// <returns>IP address</returns>
        public static string GetIP()
        {
            NetworkInterface ni = GetInterface();
            return ni.IPv4Address;
        }
    }
}