using Modicus.Interfaces;

namespace Modicus.MQTT.Interfaces
{
    internal interface IMqttSubscriber : ICommand
    {
        string Topic { get; set; }
    }
}
