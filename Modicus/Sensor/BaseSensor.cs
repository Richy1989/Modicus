using System.Threading;
using Modicus.MQTT.Interfaces;
using Modicus.Sensor.Interfaces;

namespace Modicus.Sensor
{
    internal abstract class BaseSensor : ISensor
    {
        internal Thread sensorThread;

        public string Name { get; set; }
        public int MeasurementInterval { get; set; }
        public string Type { get; set; }

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
    }
}