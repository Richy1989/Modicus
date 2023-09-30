using System;
using System.Collections;
using Modicus.Commands;
using Modicus.Interfaces;

namespace Modicus.Manager
{
    internal class CommandManager
    {
        private MqttManager mqttManager;
        public CmdMeasurementInterval CmdMeasurementInterval { get; }
        public CmdMqttClientID CmdMQTTClientID { get; }
        public CmdMqttSendInterval CmdMqttSendInterval { get; }
        public CmdMqttOnOff CmdMqttOnOff { get; }
        public IDictionary CommandCapableManagers { get; }

        public CommandManager(ISettingsManager settingsManager, MqttManager mqttManager)
        {
            this.mqttManager = mqttManager;
            CommandCapableManagers = new Hashtable();

            CmdMeasurementInterval = new CmdMeasurementInterval("MeasurementInterval", settingsManager);
            CmdMQTTClientID = new CmdMqttClientID("MqttClientId", settingsManager);
            CmdMqttSendInterval = new CmdMqttSendInterval("MqttSendInterval", settingsManager);
            CmdMqttOnOff = new CmdMqttOnOff("MqttOnOff", settingsManager, mqttManager);
        }

        public void AddCommandCapableManager(Type type, ICommandCapable commandCapable)
        {
            CommandCapableManagers.Add(type, commandCapable);
        }

        public void SetMqttCommands()
        {
            foreach (Type item in CommandCapableManagers.Keys)
            {
                ((ICommandCapable)CommandCapableManagers[item]).RegisterCommand(CmdMeasurementInterval);
                ((ICommandCapable)CommandCapableManagers[item]).RegisterCommand(CmdMQTTClientID);
                ((ICommandCapable)CommandCapableManagers[item]).RegisterCommand(CmdMqttSendInterval);
                ((ICommandCapable)CommandCapableManagers[item]).RegisterCommand(CmdMqttOnOff);
            }
        }
    }
}