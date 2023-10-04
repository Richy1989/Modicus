using System;
using System.Collections.Generic;
using System.Text;
using Modicus.MQTT.Interfaces;
using Modicus.Sensor.Interfaces;

namespace Modicus.Manager.Interfaces
{
    internal interface IBusDeviceManager
    {
        void AddSensor(ISensor sensor);

        void StartSensor(ISensor sensor);

        ISensor GetSensor(string name);
    }
}
