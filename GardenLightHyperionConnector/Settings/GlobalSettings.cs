using System;
using nanoFramework.Hardware.Esp32;

namespace Modicus.Settings
{
    public class GlobalSettings
    {
        //Default Measurement Interval is 2 Minutes
        public TimeSpan MeasurementInterval { get; set; } = TimeSpan.FromSeconds(1);
        public bool UseBME208 { get; set; } = true;

        public byte I2C_SDA { get; set; } = Gpio.IO21;
        public byte I2C_SCL { get; set; } = Gpio.IO22;

        public WifiSettings WifiSettings { get; set; } = new WifiSettings();
        public MqttSettings MqttSettings { get; set; } = new MqttSettings();
    }
}
