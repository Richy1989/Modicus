namespace Modicus.Sensor.Measurement
{
    internal class EnvironmentMeasurement : BaseMeasurement
    {
        public double Temperature { get; set; }
        public double Pressure { get; set; }
        public double Altitude { get; set; }
        public double Humidity { get; set; }
        
        public EnvironmentMeasurement(string measurmentCategory) : base(measurmentCategory)
        {
            if(string.IsNullOrEmpty(measurmentCategory))
                MeasurmentCategory = "Environment";
        }
        internal override BaseMeasurement Clone()
        {
            EnvironmentMeasurement cloned = new(MeasurmentCategory)
            {
                Temperature = Temperature,
                Pressure = Pressure,
                Altitude = Altitude,
                Humidity = Humidity
            };
            return cloned;
        }
    }
}