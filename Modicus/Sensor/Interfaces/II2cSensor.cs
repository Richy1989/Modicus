using System.Device.I2c;

namespace Modicus.Sensor.Interfaces
{
    /// <summary>
    /// Inteface for config data for an I2C Sensor
    /// </summary>
    internal interface II2cSensor : ISensor
    {
        int SclPin { get; set; }
        int SdaPin { get; set; }
        int BusID { get; set; }
        I2cBusSpeed I2cBusSpeed { get; set; }
        int DeviceAddress { get; set; }
    }
}