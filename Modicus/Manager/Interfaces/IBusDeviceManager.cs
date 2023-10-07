using System.Collections;
using Modicus.Sensor.Interfaces;

namespace Modicus.Manager.Interfaces
{
    internal interface IBusDeviceManager
    {
        IDictionary SupportedSensors { get; }
        IDictionary ConfiguredSensors { get; }

        void AddSensor(ISensor sensor);

        void StartSensor(ISensor sensor);

        ISensor GetSensor(string name);

        ISensor GetSensorFromName(string name);
        void StopSensor(ISensor sensor);

        void DeleteSensor(ISensor sensor);
    }
}