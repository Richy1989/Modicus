using System;

namespace Modicus.MQTT
{
    public class WiFiMessage
    {
        public string SSId {  get; set; }
        public string BSSId { get; set; }
        public int Channel {  get; set; }
        public TimeSpan DownTime { get; set; }
    }
}
