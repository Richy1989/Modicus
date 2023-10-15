using System;
using System.Net;

namespace Modicus.Settings
{
    public class MqttSettings
    {
        //Default Measurement Interval is 10 Seconds
        public TimeSpan SendInterval { get; set; } = TimeSpan.FromSeconds(10);

        public bool ConnectToMqtt { get; set; }
        public string MqttUserName { get; set; }
        public string MqttPassword { get; set; }
        public string MqttClientID { get; set; }
        public string MqttHostName { get; set; }
        public int MqttPort { get; set; } = 1883;
    }
}