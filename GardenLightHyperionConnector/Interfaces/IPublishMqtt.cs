using System;
using System.Collections;
using System.Text;
using GardenLightHyperionConnector.MQTT;

namespace NFApp1.Interfaces
{
    public  interface IPublishMqtt
    {
        void Publish(string topic, string message);
        IDictionary Messages { get; set; }
        MainMqttMessage MainMqttMessage { get; set; }
    }
}
