using System.Threading;
using Modicus.MQTT.Interfaces;
using Modicus.Sensor.Interfaces;

namespace Modicus.Sensor
{
    /// <summary>Base sensor class, use this class as base for all kind of sensors.</summary>
    internal abstract class BaseSensor : ISensor
    {
        /// The sensor token source is for cancellation within the sensor. e.g Stop Function
        internal CancellationTokenSource sensorTokenSource;
        internal CancellationToken sensorToken;
        internal Thread sensorThread;

        public string Name { get; set; }
        public bool IsRunning { get; protected set; }
        public int MeasurementInterval { get; set; }
        public string Type { get; set; }

        /// <summary>Constructor for Base Sensor class</summary>
        internal BaseSensor()
        {
        }

        /// <summary>Function to be called once to configure the sensor.</summary>
        /// <param name="sensorData"></param>
        public abstract void Configure(IPublishMqtt publisher);

        /// <summary>Starts the measurement thread.</summary>
        protected abstract void DoMeasurement(CancellationToken token);

        /// <summary>Function that is executed after the measurement task has started.</summary>
        protected abstract void PostStartMeasurement();

        /// <summary>Implement this function with the logic to do the active measurement.</summary>
        /// <param name="token">The global cancelltation token.</param>
        public void StartMeasurement(CancellationToken token)
        {
            sensorTokenSource = new CancellationTokenSource();
            sensorToken = sensorTokenSource.Token;

            sensorThread = new Thread(() =>
            {
                IsRunning = true;
                DoMeasurement(token);
                IsRunning = false;
            });
            sensorThread.Start();
            PostStartMeasurement();
        }

        /// <summary>Disposes the object safely.</summary>
        public abstract void Dispose();

        /// <summary>Stops the sensor.</summary>
        public abstract void StopSensor();
    }
}