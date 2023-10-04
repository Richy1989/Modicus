using System.Collections;
using Modicus.Sensor.Interfaces;

namespace Modicus.Manager.Interfaces
{
    internal interface IBusDeviceManager
    {
        IList SupportedSensors { get; }

        void AddSensor(ISensor sensor);

        void StartSensor(ISensor sensor);

        ISensor GetSensor(string name);
    }
}