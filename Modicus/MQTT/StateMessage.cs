using System;

namespace Modicus.MQTT
{
    public class StateMessage
    {
        public DateTime Time { get; set; }
        public TimeSpan Uptime { get; set; }
        public double UptimeSec { get; set; }

        public WiFiMessage WiFi { get; set; } = new WiFiMessage();
    }
}
