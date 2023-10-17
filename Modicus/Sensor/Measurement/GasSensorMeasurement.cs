namespace Modicus.Sensor.Measurement
{
    internal class GasSensorMeasurement : BaseMeasurement
    {
        public double TotalVolatileOrganicCompound { get; set; }
        public double eCO2 { get; set; }

        /// <summary>Initializes a new instance of the <see cref="GasSensorMeasurement"/> class.</summary>
        /// <param name="measurmentCategory">The measurment category.</param>
        public GasSensorMeasurement(string measurmentCategory) : base(measurmentCategory)
        {
            if (string.IsNullOrEmpty(measurmentCategory))
                MeasurmentCategory = "Gas";
        }

        /// <summary>Clones this instance.</summary>
        internal override BaseMeasurement Clone()
        {
            GasSensorMeasurement cloned = new(MeasurmentCategory)
            {
                TotalVolatileOrganicCompound = TotalVolatileOrganicCompound,
                eCO2 = eCO2
            };
            return cloned;
        }
    }
}