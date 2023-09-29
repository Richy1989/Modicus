﻿using Modicus.MQTT;

namespace Modicus.Interfaces
{
    public interface IPublishMqtt
    {
        void Publish(string topic, string message);

        MainMqttMessage MainMqttMessage { get; set; }
        StateMessage State { get; set; }
    }
}