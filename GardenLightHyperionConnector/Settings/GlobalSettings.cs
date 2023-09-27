using nanoFramework.Hardware.Esp32;

namespace GardenLightHyperionConnector.Settings
{
    public class GlobalSettings
    {
        //Default Measurement Interval is 2 Minutes
        public int MeasurementInterval { get; set; } = 1000;//1000 * 60 * 2;
        public bool UseBME208 { get; set; } = true;

        public byte I2C_SDA { get; set; } = Gpio.IO21;
        public byte I2C_SCL { get; set; } = Gpio.IO22;

        public WifiSettings WifiSettings { get; set; } = new WifiSettings();
        public MqttSettings MqttSettings { get; set; } = new MqttSettings();
    }
}
