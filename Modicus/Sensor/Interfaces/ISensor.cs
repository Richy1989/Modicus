using System;
using System.Collections;
using System.Threading;
using Modicus.MQTT.Interfaces;
using Modicus.Sensor.Measurement;

namespace Modicus.Sensor.Interfaces
{
    /// <summary>The base interface for all sensors.</summary>
    internal interface ISensor : IDisposable
    {
        bool IsRunning { get; }
        string Type { get; set; }
        string Name { get; set; }
        int MeasurementInterval { get; set; }

        BaseMeasurement Measurement { get; set; }

        event MeasurementAvailableHandler MeasurementAvailable;

        /// <summary>Function to be called once to configure the sensor.</summary>
        void Configure();

        /// <summary>Starts the measurement thread.</summary>
        void StartMeasurement(CancellationToken token);

        /// <summary>Stops the sensor.</summary>
        void StopSensor();

        /// <summary>Returns a list of Measurements this Sensor depends on.</summary>
        IList DependsOnMeasurement();

        /// <summary>Injects the depended measurement.</summary>
        /// <param name="measurement">The measurement.</param>
        void InjectDependedMeasurement(BaseMeasurement measurement);
    }
}