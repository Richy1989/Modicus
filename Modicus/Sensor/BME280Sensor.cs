using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Bmxx80;
using Iot.Device.Common;
using Modicus.EventArgs;
using Modicus.Sensor.Measurement;
using UnitsNet;

namespace Modicus.Sensor
{
    internal class BME280Sensor : BaseI2cSensor
    {
        private Bme280 i2CBme280;
        private Pressure defaultSeaLevelPressure;

        /// <summary>Initializes a new instance of the <see cref="BME280Sensor"/> class.</summary>
        internal BME280Sensor() : base()
        { }

        /// <summary>Returns a list of Measurements this Sensor depends on.</summary>
        public override IList DependsOnMeasurement() => null;

        /// <summary>Configures the Sensor.</summary>
        public override void Configure()
        {
            base.Configure();

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
        protected override void DoMeasurement(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !sensorToken.IsCancellationRequested)
            {
                // Perform a synchronous measurement
                var readResult = i2CBme280.Read();

                // Note that if you already have the pressure value and the temperature, you could also calculate altitude by using
                // var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
                i2CBme280.TryReadAltitude(defaultSeaLevelPressure, out var altValue);

                var measurement = new EnvironmentMeasurement(MeasurementCategory);

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

                OnMeasurementAvailable(this, new MeasurementAvailableEventArgs(this, measurement));

                Thread.Sleep(MeasurementInterval);
            }
        }

        /// <summary>Function that is executed after the measurement task has started.</summary>
        protected override void PostStartMeasurement()
        { }

        /// <summary>Disposes the sensor.</summary>
        public override void Dispose() => i2CBme280?.Dispose();

        /// <summary>Stops the sensor.</summary>
        public override void StopSensor()
        {
            sensorTokenSource?.Cancel();
            IsRunning = false;
        }

        /// <summary>Injects the depended measurement.</summary>
        /// <param name="measurement">The measurement.</param>
        public override void InjectDependedMeasurement(BaseMeasurement measurement)
        {
            throw new NotImplementedException();
        }
    }
}