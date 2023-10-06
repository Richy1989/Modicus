using System;

namespace Modicus.Commands.Interfaces
{
    internal interface ICommandManager
    {
        CmdMeasurementInterval CmdMeasurementInterval { get; }
        CmdMqttClientID CmdMQTTClientID { get; }
        CmdMqttSendInterval CmdMqttSendInterval { get; }
        CmdMqttOnOff CmdMqttOnOff { get; }
        CmdSystemReboot CmdSystemReboot { get; }
        CmdCreateI2CSensor CmdCreateI2CSensor { get; }

        void AddCommandCapableManager(Type type, ICommandCapable commandCapable);

        void SetMqttCommands();
    }
}