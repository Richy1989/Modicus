using System;
using System.Threading;
using Modicus.MQTT.Interfaces;

namespace Modicus.Sensor.Interfaces
{
    /// <summary>The base interface for all sensors.</summary>
    internal interface ISensor : IDisposable
    {
        bool IsRunning { get; }
        string Type { get; set; }
        string Name { get; set; }
        int MeasurementInterval { get; set; }

        event MeasurementAvailableHandler MeasurementAvailable;

        /// <summary>Function to be called once to configure the sensor.</summary>
        void Configure();

        /// <summary>Starts the measurement thread.</summary>
        void StartMeasurement(CancellationToken token);

        /// <summary>Stops the sensor.</summary>
        void StopSensor();
    }
}