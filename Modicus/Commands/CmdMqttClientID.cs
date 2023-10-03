using System;
using System.Diagnostics;
using System.Threading;
using Modicus.Manager.Interfaces;
using nanoFramework.Json;

namespace Modicus.Commands
{
    //With this command the Measurement Interval can be set in Seconds
    internal class CmdMqttClientID : BaseCommand
    {
        private readonly ISettingsManager settingsManager;

        /// <summary>
        /// Instantiate the command to set MQTT CLient ID
        /// </summary>
        /// <param name="settingsManager"></param>
        public CmdMqttClientID(ISettingsManager settingsManager) : base()
        {
            Topic = settingsManager.GlobalSettings.CommandSettings.MqttClientIdTopic;
            this.settingsManager = settingsManager;
        }

        //Execute the command
        public void Execute(CmdMqttClientIdData data)
        {
            if (data != null)
            {
                settingsManager.GlobalSettings.MqttSettings.MqttClientID = data.ClientID;

                Thread updateSettingsThread = new(new ThreadStart(settingsManager.UpdateSettings));
                updateSettingsThread.Start();
            }
        }
        public new void Execute(string content)
        {
            CmdMqttClientIdData data = null;
            try
            {
                data = (CmdMqttClientIdData)JsonConvert.DeserializeObject(content, typeof(CmdMqttClientIdData));
                Debug.WriteLine($"New ClientID: {data.ClientID}s");
                Execute(data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in clientID set command: {ex.Message}");
            }
            base.Execute(content);
        }
    }

    //This class contraints the content of the message which will be sent via JSON
    internal class CmdMqttClientIdData
    {
        public string ClientID { get; set; }
    }
}