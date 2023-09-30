using Modicus.Commands.Interfaces;

namespace Modicus.MQTT.Interfaces
{
    internal interface IMqttManager : ICommandCapable
    {
        void InitializeMQTT();

        void StopMqtt(bool preventAutoRestart = true);
    }
}