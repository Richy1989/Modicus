﻿using System;
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

        public void Execute(CmdMqttOnOffData content)
        {
            base.mreExecute.WaitOne();

            try
            {
                
                if (content.On)
                    Debug.WriteLine("Command: Turn MQTT ON");
                else
                    Debug.WriteLine("Command: Turn MQTT OFF");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in measurement interval command: {ex.Message}");
            }

            if (content != null)
            {
                if (content.On)
                    mqttManager.InitializeMQTT();
                else
                    mqttManager.StopMqtt();
            }

            base.mreExecute.Set();
        }


        //Execute the command
        public new void Execute(string content)
        {
            CmdMqttOnOffData data = (CmdMqttOnOffData)JsonConvert.DeserializeObject(content, typeof(CmdMqttOnOffData));
            Execute(data);
            base.Execute(content);
        }
    }

    //This class contraints the content of the message which will be sent via JSON
    internal class CmdMqttOnOffData
    {
        public bool On { get; set; }
    }
}