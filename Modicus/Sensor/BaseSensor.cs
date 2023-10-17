using System.Collections;
using System.Threading;
using Modicus.EventArgs;
using Modicus.MQTT.Interfaces;
using Modicus.Sensor.Interfaces;
using Modicus.Sensor.Measurement;

namespace Modicus.Sensor
{
    // Declare the delegate (if using non-generic pattern).
    internal delegate void MeasurementAvailableHandler(object sender, MeasurementAvailableEventArgs e);

    /// <summary>Base sensor class, use this class as base for all kind of sensors.</summary>
    internal abstract class BaseSensor : ISensor
    {
        public event MeasurementAvailableHandler MeasurementAvailable;

        /// The sensor token source is for cancellation within the sensor. e.g Stop Function
        internal CancellationTokenSource sensorTokenSource;
        internal CancellationToken sensorToken;
        internal Thread sensorThread;

        public string Name { get; set; }
        public bool IsRunning { get; protected set; }
        public int MeasurementInterval { get; set; }
        public string Type { get; set; }
        public string MeasurementCategory { get; set; }

        /// <summary>Initializes a new instance of the <see cref="BaseSensor"/> class.</summary>
        internal BaseSensor()
        {
        }

        /// <summary>Returns a list of Measurements this Sensor depends on.</summary>
        public abstract IList DependsOnMeasurement();

        /// <summary>Injects the depended measurement.</summary>
        /// <param name="measurement">The measurement.</param>
        public abstract void InjectDependedMeasurement(BaseMeasurement measurement);

        /// <summary>Function to be called once to configure the sensor.</summary>
        public abstract void Configure();

        /// <summary>Measurement function which will be called within measurement thread.</summary>
        /// <param name="token">The cancellation token.</param>
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

        /// <summary>Called when [measurement available].</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MeasurementAvailableEventArgs"/> instance containing the event data.</param>
        protected virtual void OnMeasurementAvailable(object sender, MeasurementAvailableEventArgs e)
        {
            MeasurementAvailable?.Invoke(this, e);
        }
    }
}