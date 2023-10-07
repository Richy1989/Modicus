using System.Threading;
using Modicus.MQTT.Interfaces;
using Modicus.Sensor.Interfaces;

namespace Modicus.Sensor
{
    /// <summary>
    /// Base sensor class, use this class for all kind of sensors. 
    /// </summary>
    internal abstract class BaseSensor : ISensor
    {
        internal CancellationTokenSource sensorTokenSource;
        internal Thread sensorThread;
        internal CancellationToken sensorToken;

        public string Name { get; set; }
        public bool IsRunning { get; internal set; }
        public int MeasurementInterval { get; set; }
        public string Type { get; set; }

        /// <summary>Constructor for Base Sensor class</summary>
        internal BaseSensor()
        {
        }

        /// <summary>
        /// Function to be called once to configure the sensor
        /// </summary>
        /// <param name="sensorData"></param>
        public abstract void Configure(IPublishMqtt publisher);

        /// <summary>
        /// Starts the measurement thread.
        /// </summary>
        public abstract void StartMeasurement(CancellationToken token);

        /// <summary>
        /// Disposes the object safely
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Stops the sensor.
        /// </summary>
        public abstract void StopSensor();
    }
}