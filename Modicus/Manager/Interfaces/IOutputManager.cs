using System.Collections;
using Modicus.OutputDevice.Interface;
using Modicus.Sensor.Measurement;

namespace Modicus.Manager.Interfaces
{
    internal interface IOutputManager
    {
        /// <summary>Gets the output data.</summary>
        /// <value>The output data.</value>
        public IDictionary OutputData { get; }

        /// <summary>Adds new measurment data.</summary>
        /// <param name="measurement">The measurement.</param>
        void AddMeasurementData(BaseMeasurement measurement);

        /// <summary>Creates a json string of the saved measurement data.</summary>
        /// <returns>Measurement Data JSON string.</returns>
        string GetJsonString();
        /// <summary>Registers the output device.</summary>
        /// <param name="device">The device.</param>
        void RegisterOutputDevice(IOutputDevice device);

        /// <summary>Removes the output device.</summary>
        /// <param name="device">The device.</param>
        void RemoveOutputDevice(IOutputDevice device);

        /// <summary>Purges the measurement data from the OutputData.</summary>
        /// <param name="measurement">The measurement.</param>
        void PurgeMeasurementData(BaseMeasurement measurement);
    }
}