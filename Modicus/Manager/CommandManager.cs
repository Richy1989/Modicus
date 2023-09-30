using System;
using System.Collections;
using Modicus.Commands;
using Modicus.Commands.Interfaces;
using Modicus.Interfaces;
using Modicus.MQTT.Interfaces;

namespace Modicus.Manager
{
    internal class CommandManager : ICommandManager
    {
        private IMqttManager mqttManager;
        public CmdMeasurementInterval CmdMeasurementInterval { get; }
        public CmdMqttClientID CmdMQTTClientID { get; }
        public CmdMqttSendInterval CmdMqttSendInterval { get; }
        public CmdMqttOnOff CmdMqttOnOff { get; }
        private IDictionary CommandCapableManagers { get; }

        /// <summary>
        /// Instantiates a new Command Manager instance
        /// </summary>
        /// <param name="settingsManager"></param>
        /// <param name="mqttManager"></param>
        public CommandManager(ISettingsManager settingsManager,IMqttManager mqttManager)
        {
            this.mqttManager = mqttManager;
            CommandCapableManagers = new Hashtable();

            CmdMeasurementInterval = new CmdMeasurementInterval(settingsManager); ;
            CmdMQTTClientID = new CmdMqttClientID(settingsManager);
            CmdMqttSendInterval = new CmdMqttSendInterval(settingsManager);
            CmdMqttOnOff = new CmdMqttOnOff(settingsManager, mqttManager);
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