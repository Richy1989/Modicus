using System;
using Modicus.Sensor;

namespace Modicus.MQTT
{
    public class MainMqttMessage
    {
        public DateTime Time { get; set; }
        public Bmp280Measurement Environment { get; set; }
    }
}
