﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using GardenLightHyperionConnector.Manager;
using nanoFramework.WebServer;

namespace Modicus.Web
{
    [Authentication("Basic:user p@ssw0rd")]
    internal class ModicusWebpageAPI
    {
        /// <summary>
        /// Stop motor
        /// </summary>
        /// <param name="e">Web server context</param>
        [Route("save")]
        public void SaveSettings(WebServerEventArgs e)
        {
            Debug.WriteLine(e.Context.Request.RawUrl);
        }

        [Route("wifi_settings")]
        public void WifiSettings(WebServerEventArgs e)
        {
            Debug.WriteLine(e.Context.Request.RawUrl);

            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);
            var ssid = (string)hashPars["ssid"];
            var password = (string)hashPars["password"];

            Debug.WriteLine($"Wireless parameters SSID:{ssid} PASSWORD:{password}");
        }

        [Route("ip_settings")]
        public void IPSettings(WebServerEventArgs e)
        {
            Debug.WriteLine(e.Context.Request.RawUrl);

            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);
            var ip = (string)hashPars["ip-address"];
            var subnet = (string)hashPars["subnetmask"];
            var gateway = (string)hashPars["default-gateway"];

            Debug.WriteLine($"IP parameters IP:{ip} Subnetmask:{subnet} Default Gateway {gateway}");
        }

        [Route("mqtt_settings")]
        public void MqttSettings(WebServerEventArgs e)
        {
            Debug.WriteLine(e.Context.Request.RawUrl);

            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);
            var cleintID = (string)hashPars["mqtt-clientid"];
            var sendInterval = (string)hashPars["mqtt-send-interval"];

            Debug.WriteLine($"MQTT parameters ClientID:{cleintID} Send Interval:{sendInterval}");
        }

        [Route("select_section")]
        public void SelectSettings(WebServerEventArgs e)
        {
            Debug.WriteLine(e.Context.Request.RawUrl);


            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);
            var ip_address = (string)hashPars["ip_address"];
            var mqtt_settings = (string)hashPars["mqtt_settings"];
            var bwifi_settings = (string)hashPars["wifi_settings"];

            if(ip_address != null)
                WebManager.OutPutResponse(e.Context.Response, Resources.Resources.GetString(Resources.Resources.StringResources.ip_settings));

            if (mqtt_settings != null)
                WebManager.OutPutResponse(e.Context.Response, Resources.Resources.GetString(Resources.Resources.StringResources.mqtt_settings));

            if (bwifi_settings != null)
                WebManager.OutPutResponse(e.Context.Response, Resources.Resources.GetString(Resources.Resources.StringResources.wifi_settings));
        }
    }
}
