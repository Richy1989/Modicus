using Modicus.Sensor.Measurement;

namespace Modicus.EventArgs
{
    internal class MeasurementAvailableEventArgs
    {
        public object Source { get; }
        public BaseMeasurement Data { get; }

        /// <summary>Initializes a new instance of the <see cref="MeasurementAvailableEventArgs"/> class.</summary>
        /// <param name="source">The source.</param>
        /// <param name="data">The data.</param>
        public MeasurementAvailableEventArgs(object source, BaseMeasurement data)
        {
            Source = source;
            Data = data;
        }
    }
}