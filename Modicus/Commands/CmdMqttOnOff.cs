using System;
using System.Diagnostics;
using Modicus.Manager.Interfaces;
using Modicus.MQTT.Interfaces;
using nanoFramework.Json;

namespace Modicus.Commands
{
    //With this command the ;QTT Service can be turned on and off
    internal class CmdMqttOnOff : BaseCommand
    {
        private readonly IMqttManager mqttManager;

        public CmdMqttOnOff(ISettingsManager settingsManager, IMqttManager mqttManager)
        {
            this.mqttManager = mqttManager;
            Topic = settingsManager.GlobalSettings.CommandSettings.MqttOnOffTopic;
        }

        public bool Execute(CmdMqttOnOffData content)
        {
            if (content == null)
            {
                Debug.WriteLine($"Command: MQTT On/Off -> No Payload!");
                return false;
            }

            try
            {
                if (content.On)
                    mqttManager.InitializeMQTT();
                else
                    mqttManager.StopMqtt();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in turn MQTT ON/OFF command: {ex.Message}");
                return false;
            }
            
            return true;
        }

        //Execute the command
        public new void Execute(string content)
        {
            base.mreExecute.WaitOne();

            CmdMqttOnOffData data = (CmdMqttOnOffData)JsonConvert.DeserializeObject(content, typeof(CmdMqttOnOffData));
            if (Execute(data))
                base.Execute(content);

            base.mreExecute.Set();
        }
    }

    //This class contraints the content of the message which will be sent via JSON
    internal class CmdMqttOnOffData
    {
        public bool On { get; set; }
    }
}