using System;
using System.Collections;
using Modicus.Commands;
using Modicus.Interfaces;

namespace GardenLightHyperionConnector.Manager
{
    internal class CommandManager
    {
        public CmdMeasurementInterval CmdMeasurementInterval { get; }
        public CmdMQTTClientID CmdMQTTClientID { get; }
        public IDictionary CommandCapableManagers { get; }


        public CommandManager(ISettingsManager settingsManager)
        {
            CmdMeasurementInterval = new CmdMeasurementInterval("measurementinterval", settingsManager);
            CmdMQTTClientID = new CmdMQTTClientID("mqttclientid", settingsManager);

            CommandCapableManagers = new Hashtable();
        }
        public void AddCommandCapableManager(Type type, ICommandCapable commandCapable)
        {
            CommandCapableManagers.Add(type, commandCapable);
        }

        public void SetMqttCommands()
        {   
            foreach(Type item in CommandCapableManagers.Keys)
            {
                ((ICommandCapable)CommandCapableManagers[item]).RegisterCommand(CmdMeasurementInterval);
                ((ICommandCapable)CommandCapableManagers[item]).RegisterCommand(CmdMQTTClientID);
            }
        }
    }
}
