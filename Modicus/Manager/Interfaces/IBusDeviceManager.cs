using System.Collections;
using Modicus.Sensor.Interfaces;

namespace Modicus.Manager.Interfaces
{
    internal interface IBusDeviceManager
    {
        IDictionary SupportedSensors { get; }

        void AddSensor(ISensor sensor);

        void StartSensor(ISensor sensor);

        ISensor GetSensor(string name);
    }
}