using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using GardenLightHyperionConnector.Manager;
using Modicus.Manager.Interfaces;
using Modicus.Sensor.Interfaces;
using nanoFramework.WebServer;
using GC = nanoFramework.Runtime.Native.GC;

namespace Modicus.Web
{
    internal class ModicusWebpages
    {
        private readonly ISettingsManager settingsManager;
        private readonly IBusDeviceManager busDeviceManager;

        /// <summary>Creates a new ModicusWebpages instance.</summary>
        public ModicusWebpages(ISettingsManager settingsManager, IBusDeviceManager busDeviceManager)
        {
            this.settingsManager = settingsManager;
            this.busDeviceManager = busDeviceManager;
        }

        ///// <summary>
        ///// Serves the favicon
        ///// </summary>
        ///// <param name="e">Web server context</param>
        //[Route("favicon.ico")]
        //public void Favico(WebServerEventArgs e)
        //{
        //    // WebServer.SendFileOverHTTP(e.Context.Response, "favico.ico", Resources.GetBytes(Resources.BinaryResources.favico), "image/ico");
        //}

        [Route("style.css")]
        public void Style(WebServerEventArgs e)
        {
            e.Context.Response.ContentType = "text/css";
            WebServer.OutPutStream(e.Context.Response, Resources.Resources.GetString(Resources.Resources.StringResources.style));
        }

        ///// <summary>
        ///// Serves the SVG image
        ///// </summary>
        ///// <param name="e">Web server context</param>
        //[Route("image.svg")]
        //public void Image(WebServerEventArgs e)
        //{
        //    //  WebServer.SendFileOverHTTP(e.Context.Response, "image.svg", Resources.GetBytes(Resources.BinaryResources.image), "image/svg+xml");
        //}

        [Route("select_section")]
        public void SelectSettings(WebServerEventArgs e)
        {
            Debug.WriteLine(e.Context.Request.RawUrl);

            Hashtable hashPars = WebManager.ParseParamsFromStream(e.Context.Request.InputStream);
            var ip_address = (string)hashPars["ip_address"];
            var mqtt_settings = (string)hashPars["mqtt_settings"];
            var system_settings = (string)hashPars["system_settings"];
            var sensor_selection = (string)hashPars["sensor_selection"];

            if (ip_address != null)
            {
                WebManager.OutPutResponse(e.Context.Response, CreateIPSettingsPage(""));
                return;
            }

            if (mqtt_settings != null)
            {
                WebManager.OutPutResponse(e.Context.Response, CreateMQTTSettingsPage(""));
                return;
            }

            if (system_settings != null)
            {
                WebManager.OutPutResponse(e.Context.Response, CreateSystemSettingsPage(""));
                return;
            }

            if (sensor_selection != null)
            {
                WebManager.OutPutResponse(e.Context.Response, CreateSensorSettingsPage(""));
                return;
            }

            Default(e);
        }

        /// <summary>
        /// This is the default page
        /// </summary>
        /// <remarks>the / route *must* always be the last one and the last of the last controller passed
        /// to the constructor</remarks>
        /// <param name="e">Web server context</param>
        [Route("default.html"), Route("index.html"), Route("/")]
        public void Default(WebServerEventArgs e)
        {
            e.Context.Response.ContentType = "text/html";

            var message = "Welcome to Modicus ... Have fun!";
            var body = string.Format(Resources.Resources.GetString(Resources.Resources.StringResources.index), settingsManager.GlobalSettings.SystemSettings.InstanceName);

            var page = CreateSite("Modicus", body, message);

            WebServer.OutPutStream(e.Context.Response, page);
        }

        public string CreateMQTTSettingsPage(string message)
        {
            var mqttSettings = settingsManager.GlobalSettings.MqttSettings;
            var body = string.Format(Resources.Resources.GetString(Resources.Resources.StringResources.mqtt_settings),
                mqttSettings.ConnectToMqtt ? "checked" : "unchecked",
                mqttSettings.MqttHostName,
                mqttSettings.MqttPort,
                mqttSettings.MqttUserName,
                mqttSettings.MqttPassword,
                mqttSettings.MqttClientID,
                settingsManager.GlobalSettings.SendInterval.TotalSeconds);
            return CreateSite("MQTT Settings", body, message);
        }

        public string CreateIPSettingsPage(string message)
        {
            var wifiSettings = settingsManager.GlobalSettings.WifiSettings;
            var body = string.Format(Resources.Resources.GetString(Resources.Resources.StringResources.ip_settings),
                wifiSettings.Ssid,
                wifiSettings.Password,
                wifiSettings.UseDHCP ? "checked" : "unchecked",
                wifiSettings.IP,
                wifiSettings.NetworkMask,
                wifiSettings.DefaultGateway);

            return CreateSite("IP Settings", body, message);
        }

        public string CreateSystemSettingsPage(string message)
        {
            var body = string.Format(Resources.Resources.GetString(Resources.Resources.StringResources.system_settings),
                settingsManager.GlobalSettings.SystemSettings.InstanceName,
                settingsManager.GlobalSettings.SystemSettings.UseSignalling ? "checked" : "unchecked",
                settingsManager.GlobalSettings.SystemSettings.SignalGpioPin);
            return CreateSite("System Settings", body, message);
        }

        public string CreateSensorSettingsPage(string message)
        {
            StringBuilder stringBuilder = new();

            stringBuilder.Append("<header><h1>Sensor Settings</h1></header>");
            stringBuilder.Append("<form method='POST' action=\"sensor_settings\">");
            stringBuilder.Append("<input type=\"submit\" class=\"big-button\" name=\"edit_sensor\" value=\"Edit Sensors\">");
            stringBuilder.Append("<input type=\"submit\" class=\"big-button\" name=\"create_sensor\" value=\"Create Sensors\">");
            stringBuilder.Append("</form>");
            stringBuilder.Append("<br />");
            stringBuilder.Append("<br />");
            stringBuilder.Append("<form method='POST' action=\"sensor_settings\"><input class=\"save_back_submit\" type=\"submit\" value=\"Back\" name=\"back\"></form>");
            
            return CreateSite("Sensor Settings", stringBuilder.ToString(), message);
        }

        public string CreateSensorCreationSite(string message)
        {
            StringBuilder itemString = new();
            foreach (string item in busDeviceManager.SupportedSensors.Keys)
            {
                itemString.Append(string.Format("<input type=\"submit\" class=\"input_drop_down\" name=\"item\" value=\"{0}\">", item));
            }

            StringBuilder bodyString = new();
            bodyString.Append(string.Format(Resources.Resources.GetString(Resources.Resources.StringResources.select_sensor), itemString.ToString()));

            itemString.Clear();

            return CreateSite("Sensor Selection", bodyString.ToString(), message);
        }

        public string CreateSensorEditSite(string message)
        {
            StringBuilder alreadyConfigured = new();

            foreach (DictionaryEntry item in busDeviceManager.ConfiguredSensors)
            {
                ISensor sensor = busDeviceManager.GetSensorFromName(item.Key as string);
                alreadyConfigured.Append(
                    string.Format(Resources.Resources.GetString(Resources.Resources.StringResources.edit_sensor_table),
                    item.Key,
                    sensor.IsRunning ? "Yes" : "No",
                    item.Key));
            }

            string site = string.Format(Resources.Resources.GetString(Resources.Resources.StringResources.edit_sensor), alreadyConfigured.ToString());
            return CreateSite("Edit Sensors", site, message);
        }

        public string CreateI2CSettingsSite(string message, string sensortype)
        {
            var body = string.Format(Resources.Resources.GetString(Resources.Resources.StringResources.i2csettings), sensortype, sensortype);

            return CreateSite("I2C Settings", body, message);
        }

        public string CreateSite(string headmessage, string body, string message)
        {
            var page = string.Format(Resources.Resources.GetString(Resources.Resources.StringResources.head), headmessage, body, message);
            return page;
        }
    }
}