using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Modicus.MQTT.Interfaces;

namespace Modicus.Sensor.Interfaces
{
    /// <summary>
    /// The base interface for all sensors
    /// </summary>
    internal interface ISensor : IDisposable
    {
        string Type { get; set; }
        string Name { get; set; }
        int MeasurementInterval { get; set; }

        /// <summary>
        /// Function to be called once to configure the sensor
        /// </summary>
        /// <param name="sensorData"></param>
        void Configure(IPublishMqtt publisher);

        /// <summary>
        /// Starts the measurement thread
        /// </summary>
        void StartMeasurement(CancellationToken token);

        /// <summary>Stops the sensor</summary>
        void StopSensor();
    }
}