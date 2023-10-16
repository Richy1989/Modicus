using Modicus.Sensor.Measurement;

namespace Modicus.EventArgs
{
    internal class MeasurementAvailableEventArgs
    {
        public object Source { get; }
        public BaseMeasurement Data { get; }

        public MeasurementAvailableEventArgs(object source, BaseMeasurement data)
        {
            Source = source;
            Data = data;
        }
    }
}