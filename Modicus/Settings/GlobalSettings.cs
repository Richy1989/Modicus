using System;

namespace Modicus.Settings
{
    internal class GlobalSettings
    {
        //Indicated if this is a new initialized instance to set default values
        public DateTime StartupTime { get; set; }

        public bool IsFreshInstall { get; set; }
        public string InstanceName { get; set; }

        //Default Measurement Interval
        public TimeSpan MeasurementInterval { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan SendInterval { get; set; } = TimeSpan.FromSeconds(10);

        public WifiSettings WifiSettings { get; set; } = new WifiSettings();
        public MqttSettings MqttSettings { get; set; } = new MqttSettings();
        public SystemSettings SystemSettings { get; set; } = new SystemSettings();
    }
}