using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using GardenLightHyperionConnector.Settings;
using Iot.Device.Bmxx80;
using Iot.Device.Common;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Json;
using NFApp1.Interfaces;
using UnitsNet;

namespace GardenLightHyperionConnector.Sensor
{
    public class BME280Sensor : IDisposable
    {
        private Bme280 i2CBme280;
        private Pressure defaultSeaLevelPressure;
        private CancellationToken token;
        private IPublishMqtt mqttPublisher;
        private I2cConnectionSettings i2cSettings;
        private I2cDevice i2cDevice;
        private GlobalSettings globalSettings;

        public int MeasurementIntervall { get; }

        public BME280Sensor(IPublishMqtt mqttPublisher, GlobalSettings settings, CancellationToken token)
        {
            this.token = token;
            this.mqttPublisher = mqttPublisher;
            this.globalSettings = settings;
            MeasurementIntervall = settings.MeasurementInterval;
        }

        public void Init()
        {
            //////////////////////////////////////////////////////////////////////GlobalSettings.I2CSDA, GlobalSettings.I2CSCL
            // when connecting to an ESP32 device, need to configure the I2C GPIOs
            // used for the bus
            Configuration.SetPinFunction(globalSettings.I2C_SDA, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(globalSettings.I2C_SCL, DeviceFunction.I2C1_CLOCK);


            // set this to the current sea level pressure in the area for correct altitude readings
            defaultSeaLevelPressure = WeatherHelper.MeanSeaLevel;

            // bus id on the MCU
            const int busId = 1;

            i2cSettings = new I2cConnectionSettings(busId, Bme280.SecondaryI2cAddress, I2cBusSpeed.StandardMode);
            i2cDevice = I2cDevice.Create(i2cSettings);

            try
            {
                i2CBme280 = new Bme280(i2cDevice);
                i2CBme280.Reset();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            // set higher sampling
            i2CBme280.TemperatureSampling = Sampling.UltraHighResolution;
            i2CBme280.PressureSampling = Sampling.UltraHighResolution;
            i2CBme280.HumiditySampling = Sampling.UltraHighResolution;
        }

        public void DoMeasurement()
        {
            while (!token.IsCancellationRequested)
            {
                // Perform a synchronous measurement
                var readResult = i2CBme280.Read();

                // Note that if you already have the pressure value and the temperature, you could also calculate altitude by using
                // var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
                i2CBme280.TryReadAltitude(defaultSeaLevelPressure, out var altValue);

                Bmp280Measurement measurement = new ();

                if (readResult.TemperatureIsValid)
                {
                    Debug.WriteLine($"Temperature: {readResult.Temperature.DegreesCelsius}\u00B0C");
                    measurement.Temperature = readResult.Temperature.DegreesCelsius;
                }
                if (readResult.PressureIsValid)
                {
                    Debug.WriteLine($"Pressure: {readResult.Pressure.Hectopascals}hPa");
                    measurement.Pressure = readResult.Pressure.Hectopascals;
                }

                if (readResult.TemperatureIsValid && readResult.PressureIsValid)
                {
                    Debug.WriteLine($"Altitude: {altValue.Meters}m");
                    measurement.Altitude = altValue.Meters;
                }

                if (readResult.HumidityIsValid)
                {
                    Debug.WriteLine($"Relative humidity: {readResult.Humidity.Percent}%");
                    measurement.Humidity = readResult.Humidity.Percent;
                }

                //// WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
                //if (readResult.TemperatureIsValid && readResult.HumidityIsValid)
                //{
                //    Debug.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(readResult.Temperature, readResult.Humidity).DegreesCelsius}\u00B0C");
                //    Debug.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(readResult.Temperature, readResult.Humidity).DegreesCelsius}\u00B0C");
                //}

                if (readResult.TemperatureIsValid || readResult.PressureIsValid)
                {
                    var message = JsonConvert.SerializeObject(measurement);
                    Debug.WriteLine($"BMP208 Measurement Value: {message}");
                    //mqttPublisher.Publish("SENSOR/modicus_sensorrange_livingroom", message);
                    mqttPublisher.MainMqttMessage.Environment = measurement;
                }

                Thread.Sleep(MeasurementIntervall);
            }
        }

        public void Dispose()
        {
            i2CBme280.Dispose();
        }
    }
}
