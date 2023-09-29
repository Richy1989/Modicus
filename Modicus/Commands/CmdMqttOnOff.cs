using System;
using System.Diagnostics;
using System.Threading;
using Modicus.Interfaces;
using Modicus.Manager;
using nanoFramework.Json;

namespace Modicus.Commands
{
    //With this command the ;QTT Service can be turned on and off
    internal class CmdMqttOnOff : BaseCommand
    {
        private readonly ISettingsManager settingsManager;
        private readonly MqttManager mqttManager;

        public CmdMqttOnOff(string topic, ISettingsManager settingsManager, MqttManager mqttManager) : base(topic)
        {
            this.mqttManager = mqttManager;
            this.settingsManager = settingsManager;
        }

        //Execute the command
        public new void Execute(string content)
        {
            CmdMqttOnOffData data = null;
            try
            {
                data = (CmdMqttOnOffData)JsonConvert.DeserializeObject(content, typeof(CmdMqttOnOffData));
                if (data.On)
                    Debug.WriteLine("Command: Turn MQTT ON");
                else
                    Debug.WriteLine("Command: Turn MQTT OFF");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in measurement interval command: {ex.Message}");
            }

            if (data != null)
            {
                if (data.On)
                    mqttManager.InitializeMQTT();
                else
                    mqttManager.StopMqtt();
            }

            base.Execute(content);
        }
    }

    //This class contraints the content of the message which will be sent via JSON
    internal class CmdMqttOnOffData
    {
        public bool On { get; set; }
    }
}