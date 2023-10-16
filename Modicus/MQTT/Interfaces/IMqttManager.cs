using Modicus.Commands.Interfaces;

namespace Modicus.MQTT.Interfaces
{
    internal interface IMqttManager
    {
        void InitializeMQTT();

        void StopMqtt(bool preventAutoRestart = true);
    }
}