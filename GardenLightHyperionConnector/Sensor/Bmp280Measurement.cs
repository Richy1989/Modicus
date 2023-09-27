using NFApp1.MQTT.Interfaces;

namespace GardenLightHyperionConnector.Sensor
{
    public class Bmp280Measurement// : IMessageBase
    {
       // public string Topic { get; set; } = "modicus/sensorrange_livingroom";

        public double Temperature { get; set; }
        public double Pressure { get; set; }
        public double Altitude { get; set; }
        public double Humidity { get; set; }
    }
}
