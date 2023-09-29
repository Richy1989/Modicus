using System;
using System.Diagnostics;
using System.Threading;
using Modicus.Interfaces;
using Modicus.MQTT.Interfaces;
using Modicus.Settings;
using nanoFramework.Json;

namespace GardenLightHyperionConnector.Commands
{
    //With this command the Measurement Interval can be set in Seconds
    internal class CmdMeasurementInterval : IMqttSubscriber
    {
        private readonly ISettingsManager settingsManager;
        public string Topic { get; set; }

        public CmdMeasurementInterval(ISettingsManager settingsManager)
        { 
            this.settingsManager = settingsManager;
        }

        //Execute the command
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
            {
                settingsManager.GlobalSettings.MeasurementInterval = TimeSpan.FromSeconds(data.Interval);

                Thread updateSettingsThread = new(new ThreadStart(settingsManager.UpdateSettings));
                updateSettingsThread.Start();
            }
        }
    }

    //This class contraints the content of the message which will be sent via JSON
    internal class CmdMeasurementIntervalData
    {
        public int Interval { get; set; }
    }
}
