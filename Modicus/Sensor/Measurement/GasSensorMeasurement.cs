namespace Modicus.Sensor.Measurement
{
    internal class GasSensorMeasurement : BaseMeasurement
    {
        public double TotalVolatileOrganicCompound { get; set; }
        public double eCO2 { get; set; }

        public GasSensorMeasurement()
        {
            MeasurmentCategory = "Gas";
        }

        internal override BaseMeasurement Clone()
        {
            GasSensorMeasurement cloned = new()
            {
                TotalVolatileOrganicCompound = TotalVolatileOrganicCompound,
                eCO2 = eCO2
            };
            return cloned;
        }
    }
}