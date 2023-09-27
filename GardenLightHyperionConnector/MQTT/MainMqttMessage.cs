using System;
using GardenLightHyperionConnector.Sensor;

namespace GardenLightHyperionConnector.MQTT
{
    public class MainMqttMessage
    {
        public DateTime Time { get; set; }
        public Bmp280Measurement Environment { get; set; }
    }
}
