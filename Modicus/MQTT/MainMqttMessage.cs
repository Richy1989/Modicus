using System;
using Modicus.Sensor;

namespace Modicus.MQTT
{
    public class MainMqttMessage
    {
        public DateTime Time { get; set; }
        public EnvironmentData Environment { get; set; }
    }
}
