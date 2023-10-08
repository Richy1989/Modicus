using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Bmxx80;
using Iot.Device.Common;
using Modicus.MQTT.Interfaces;
using nanoFramework.Json;
using UnitsNet;

namespace Modicus.Sensor
{
    internal class BME280Sensor : BaseI2cSensor
    {
        private Bme280 i2CBme280;
        private Pressure defaultSeaLevelPressure;
        private IPublishMqtt mqttPublisher;

        /// <summary>Creates new instamce of BME280 Sensor</summary>
        internal BME280Sensor() : base()
        { }

        /// <summary>Configures the Sensor</summary>
        /// <param name="publisher"></param>
        public override void Configure(IPublishMqtt publisher)
        {
            base.Configure(publisher);

            this.mqttPublisher = publisher;

            //set this to the current sea level pressure in the area for correct altitude readings
            defaultSeaLevelPressure = WeatherHelper.MeanSeaLevel;

            try
            {
                i2CBme280 = new Bme280(GetI2cDevice());
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

            i2CBme280.SetPowerMode(Iot.Device.Bmxx80.PowerMode.Bmx280PowerMode.Normal);
        }

        /// <summary>Starts the measurement thread.</summary>
        public override void StartMeasurement(CancellationToken token)
        {
            sensorTokenSource = new CancellationTokenSource();
            sensorToken = sensorTokenSource.Token;

            sensorThread = new Thread(() =>
            {
                while (!token.IsCancellationRequested && !sensorToken.IsCancellationRequested)
                {
                    IsRunning = true;

                    // Perform a synchronous measurement
                    var readResult = i2CBme280.Read();

                    // Note that if you already have the pressure value and the temperature, you could also calculate altitude by using
                    // var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
                    i2CBme280.TryReadAltitude(defaultSeaLevelPressure, out var altValue);

                    EnvironmentData measurement = mqttPublisher.MainMqttMessage.Environment;

                    if (measurement != null)
                    {
                        if (readResult.TemperatureIsValid)
                        {
                            //Debug.WriteLine($"Temperature: {readResult.Temperature.DegreesCelsius}\u00B0C");
                            measurement.Temperature = readResult.Temperature.DegreesCelsius;
                        }
                        if (readResult.PressureIsValid)
                        {
                            //Debug.WriteLine($"Pressure: {readResult.Pressure.Hectopascals}hPa");
                            measurement.Pressure = readResult.Pressure.Hectopascals;
                        }

                        if (readResult.TemperatureIsValid && readResult.PressureIsValid)
                        {
                            //Debug.WriteLine($"Altitude: {altValue.Meters}m");
                            measurement.Altitude = altValue.Meters;
                        }

                        if (readResult.HumidityIsValid)
                        {
                            //Debug.WriteLine($"Relative humidity: {readResult.Humidity.Percent}%");
                            measurement.Humidity = readResult.Humidity.Percent;
                        }
                    }
                    Thread.Sleep(MeasurementInterval);
                }
                IsRunning = false;  
            });
            sensorThread.Start();
        }

        /// <summary>Disposes the sensor.</summary>
        public override void Dispose() => i2CBme280?.Dispose();

        /// <summary>Stops the sensor.</summary>
        public override void StopSensor()
        {
            sensorTokenSource?.Cancel();

            if (mqttPublisher != null)
                mqttPublisher.MainMqttMessage.Environment = null;

            IsRunning = false;
        }
    }
}