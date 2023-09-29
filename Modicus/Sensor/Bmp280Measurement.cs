namespace Modicus.Sensor
{
    public class Bmp280Measurement
    {
        public double Temperature { get; set; }
        public double Pressure { get; set; }
        public double Altitude { get; set; }
        public double Humidity { get; set; }
    }
}