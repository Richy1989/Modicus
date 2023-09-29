using System;
using System.Diagnostics;
using System.Threading;
using Modicus.Interfaces;
using nanoFramework.Json;

namespace Modicus.Commands
{
    //With this command the Measurement Interval can be set in Seconds
    internal class CmdMQTTClientID : BaseCommand
    {
        private readonly ISettingsManager settingsManager;

        /// <summary>
        /// Instantiate the command to set MQTT CLient ID
        /// </summary>
        /// <param name="settingsManager"></param>
        public CmdMQTTClientID(string topic, ISettingsManager settingsManager) : base(topic)
        {
            this.settingsManager = settingsManager;
        }

        //Execute the command
        public new void Execute(string content)
        {
            CmdMQTTClientIDData data = null;
            try
            {
                data = (CmdMQTTClientIDData)JsonConvert.DeserializeObject(content, typeof(CmdMQTTClientIDData));
                Debug.WriteLine($"New ClientID: {data.ClientID}s");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in clientID set command: {ex.Message}");
            }

            if (data != null)
            {
                settingsManager.GlobalSettings.MqttSettings.MqttClientID = data.ClientID;

                Thread updateSettingsThread = new(new ThreadStart(settingsManager.UpdateSettings));
                updateSettingsThread.Start();
            }

            base.Execute(content);
        }
    }

    //This class contraints the content of the message which will be sent via JSON
    internal class CmdMQTTClientIDData
    {
        public string ClientID { get; set; }
    }
}