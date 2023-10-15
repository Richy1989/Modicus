using System;
using System.Collections;
using Modicus.Commands;
using Modicus.Commands.Interfaces;
using Modicus.Manager.Interfaces;
using Modicus.MQTT.Interfaces;
using Modicus.Wifi.Interfaces;

namespace Modicus.Manager
{
    internal class CommandManager : ICommandManager
    {
        private readonly IMqttManager mqttManager;
        public CmdMeasurementInterval CmdMeasurementInterval { get; }
        public CmdMqttClientID CmdMQTTClientID { get; }
        public CmdMqttSendInterval CmdMqttSendInterval { get; }
        public CmdMqttOnOff CmdMqttOnOff { get; }
        public CmdSystemReboot CmdSystemReboot { get; }
        public CmdCreateI2CSensor CmdCreateI2CSensor { get; }
        public CmdSensorOnOff CmdSensorOnOff { get; }
        public CmdWifiControl CmdWifiControl { get; }
        private IDictionary CommandCapableManagers { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandManager"/> class.
        /// </summary>
        /// <param name="settingsManager">The settings manager.</param>
        /// <param name="mqttManager">The MQTT manager.</param>
        /// <param name="busDeviceManager">The bus device manager.</param>
        /// <param name="wiFiManager">The wi fi manager.</param>
        public CommandManager(ISettingsManager settingsManager, IMqttManager mqttManager, IBusDeviceManager busDeviceManager, IWiFiManager wiFiManager)
        {
            this.mqttManager = mqttManager;
            CommandCapableManagers = new Hashtable();

            CmdMeasurementInterval = new CmdMeasurementInterval(settingsManager); ;
            CmdMQTTClientID = new CmdMqttClientID(settingsManager);
            CmdMqttSendInterval = new CmdMqttSendInterval(settingsManager);
            CmdMqttOnOff = new CmdMqttOnOff(settingsManager, mqttManager);
            CmdSystemReboot = new CmdSystemReboot(settingsManager);
            CmdCreateI2CSensor = new CmdCreateI2CSensor(settingsManager, busDeviceManager);
            CmdSensorOnOff = new CmdSensorOnOff(settingsManager, busDeviceManager);
            CmdWifiControl = new CmdWifiControl(settingsManager, wiFiManager);
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
                ((ICommandCapable)CommandCapableManagers[item]).RegisterCommand(CmdSystemReboot);
                ((ICommandCapable)CommandCapableManagers[item]).RegisterCommand(CmdCreateI2CSensor);
                ((ICommandCapable)CommandCapableManagers[item]).RegisterCommand(CmdSensorOnOff);
                ((ICommandCapable)CommandCapableManagers[item]).RegisterCommand(CmdWifiControl);
            }
        }
    }
}