using System;
using System.Diagnostics;
using System.Threading;
using Modicus.Interfaces;
using nanoFramework.Json;

namespace Modicus.Commands
{
    //With this command the MQTT send interval can be set in seconds
    internal class CmdMqttSendInterval : BaseCommand
    {
        private readonly ISettingsManager settingsManager;

        public CmdMqttSendInterval(string topic, ISettingsManager settingsManager) : base(topic)
        {
            this.settingsManager = settingsManager;
        }

        //Execute the command
        public new void Execute(string content)
        {
            CmdMqttSendIntervalData data = null;
            try
            {
                data = (CmdMqttSendIntervalData)JsonConvert.DeserializeObject(content, typeof(CmdMqttSendIntervalData));
                Debug.WriteLine($"New MQTT send interval: {data.Interval}s");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MQTT send interval command: {ex.Message}");
            }

            if (data != null)
            {
                settingsManager.GlobalSettings.MqttSettings.SendInterval = TimeSpan.FromSeconds(data.Interval);
                Thread updateSettingsThread = new(new ThreadStart(settingsManager.UpdateSettings));
                updateSettingsThread.Start();
            }

            base.Execute(content);
        }
    }

    //This class contraints the content of the message which will be sent via JSON
    internal class CmdMqttSendIntervalData
    {
        public int Interval { get; set; }
    }
}