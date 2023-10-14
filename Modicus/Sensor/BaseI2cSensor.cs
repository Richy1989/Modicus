using System;
using System.Device.I2c;
using Modicus.MQTT.Interfaces;
using Modicus.Sensor.Interfaces;
using nanoFramework.Hardware.Esp32;

namespace Modicus.Sensor
{
    internal abstract class BaseI2cSensor : BaseSensor, II2cSensor
    {
        public int SclPin { get; set; }
        public int SdaPin { get; set; }
        public int BusID { get; set; }
        public I2cBusSpeed I2cBusSpeed { get; set; }
        public int DeviceAddress { get; set; }

        private I2cDevice i2cDevice;

        /// <summary>Initializes a new instance of the <see cref="BaseI2cSensor"/> class.</summary>
        internal BaseI2cSensor() : base()
        {
        }

        /// <summary>Configure the sensor, call this function first.</summary>
        /// <param name="publisher">The publisher.</param>
        public override void Configure(IPublishMqtt publisher)
        {
            //////////////////////////////////////////////////////////////////////
            // when connecting to an ESP32 device, need to configure the I2C GPIOs
            // used for the bus
            //////////////////////////////////////////////////////////////////////
            Configuration.SetPinFunction(SdaPin, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(SclPin, DeviceFunction.I2C1_CLOCK);

            var i2cSettings = new I2cConnectionSettings(BusID, DeviceAddress, I2cBusSpeed);
            i2cDevice = I2cDevice.Create(i2cSettings);
        }

        /// <summary>Gets the i2c device.</summary>
        /// <returns></returns>
        public I2cDevice GetI2cDevice() => i2cDevice;

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                i2cDevice?.Dispose();
            }
        }
    }
}