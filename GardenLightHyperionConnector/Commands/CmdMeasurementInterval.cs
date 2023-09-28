using System;
using System.Diagnostics;
using Modicus.MQTT.Interfaces;
using Modicus.Settings;
using nanoFramework.Json;

namespace GardenLightHyperionConnector.Commands
{
    //With this command the Measurement Interval can be set in Seconds
    internal class CmdMeasurementInterval : IMqttSubscriber
    {
        private readonly GlobalSettings globalSettings;
        public string Topic { get; set; }

        public CmdMeasurementInterval(GlobalSettings globalSettings)
        { 
            this.globalSettings = globalSettings;
        }

        public void Execute(string content)
        {
            CmdMeasurementIntervalData data = null;
            try
            {
                data = (CmdMeasurementIntervalData)JsonConvert.DeserializeObject(content, typeof(CmdMeasurementIntervalData));
                Debug.WriteLine($"New Measurement Interval: {data.Interval}s");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in measurement interval command: {ex.Message}");
            }

            if (data != null)
                globalSettings.MeasurementInterval = TimeSpan.FromSeconds(data.Interval);
        }
    }

    internal class CmdMeasurementIntervalData
    {
        public int Interval { get; set; }
    }
}
