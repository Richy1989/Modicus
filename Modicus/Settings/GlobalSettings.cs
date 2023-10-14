﻿using System;
using nanoFramework.Hardware.Esp32;

namespace Modicus.Settings
{
    internal class GlobalSettings
    {
        //Indicated if this is a new initialized instance to set default values
        public DateTime StartupTime { get; set; }
        public bool IsFreshInstall { get; set; }

        public string InstanceName { get; set; }

        //Default Measurement Interval
        public TimeSpan MeasurementInterval { get; set; } = TimeSpan.FromSeconds(1);

        public WifiSettings WifiSettings { get; set; } = new WifiSettings();
        public MqttSettings MqttSettings { get; set; } = new MqttSettings();
    }
}