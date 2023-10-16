using Modicus.MQTT;

namespace Modicus.MQTT.Interfaces
{
    public interface IPublishMqtt
    {
        void Publish(string topic, string message);
    }
}