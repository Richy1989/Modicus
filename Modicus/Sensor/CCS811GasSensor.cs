using System.Collections;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Ccs811;
using Modicus.EventArgs;
using Modicus.Sensor.Measurement;
using UnitsNet;

namespace Modicus.Sensor
{
    internal class CCS811GasSensor : BaseI2cSensor
    {
        private readonly IList dependsOn;
        private Ccs811Sensor sensor;

        /// <summary>Initializes a new instance of the <see cref="CCS811GasSensor"/> class.</summary>
        public CCS811GasSensor() : base()
        {
            dependsOn = new ArrayList
            {
                typeof(EnvironmentMeasurement)
            };
        }

        /// <summary>Configures the CCS811 Gas Sensor.</summary>
        public override void Configure()
        {
            base.Configure();

            sensor = new Ccs811Sensor(GetI2cDevice())
            {
                OperationMode = OperationMode.ConstantPower1Second
            };

            Debug.WriteLine($"CCS811 Bootloader Version: {sensor.BootloaderVersion}");
            Debug.WriteLine($"CCS811 Application Version: {sensor.ApplicationVersion}");
            Debug.WriteLine($"CCS811 Hardware Version: {sensor.HardwareVersion}");
        }

        /// <summary>Returns a list of Measurements this Sensor depends on.</summary>
        public override IList DependsOnMeasurement() => dependsOn;

        /// <summary>Injects the depended measurement.</summary>
        /// <param name="measurement">The measurement.</param>
        public override void InjectDependedMeasurement(BaseMeasurement measurement)
        {
            if (measurement.GetType() == typeof(EnvironmentMeasurement))
            {
                AdjustTemperatureHumidity((EnvironmentMeasurement)measurement);
            }
        }

        /// <summary>
        /// Starts the the Measurement.
        /// Implement this function with the logic to do the active measurement.
        /// </summary>
        /// <param name="token"></param>
        protected override void DoMeasurement(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !sensorToken.IsCancellationRequested)
            {
                while (!sensor.IsDataReady && !token.IsCancellationRequested && !sensorToken.IsCancellationRequested)
                {
                    Thread.Sleep(1000);
                }

                var success = sensor.TryReadGasData(out VolumeConcentration eCO2, out VolumeConcentration eTVOC, out ElectricCurrent curr, out int adc);

                if (!success)
                {
                    var error = sensor.Error;
                    Debug.WriteLine($"Gas Sensor Measurement Error: {error}");
                }
                else
                {
                    //Debug.WriteLine($"Success: {success}, eCO2: {eCO2.PartsPerMillion} ppm, eTVOC: {eTVOC.PartsPerBillion} ppb, Current: {curr.Microamperes} µA, ADC: {adc} = {adc * 1.65 / 1023} V.");
                    //this.eCO2 = eCO2.PartsPerMillion;
                    //this.eTVOC = eTVOC.PartsPerBillion;
                    //this.Current = curr.Microamperes;
                    //this.ADC = adc * 1.65 / 1023;

                    BaseMeasurement measurement = new GasSensorMeasurement(MeasurementCategory)
                    {
                        eCO2 = eCO2.PartsPerMillion,
                        TotalVolatileOrganicCompound = eTVOC.PartsPerBillion
                    };
                    Measurement = measurement;

                    OnMeasurementAvailable(this, new MeasurementAvailableEventArgs(this, measurement));
                }
                Thread.Sleep(MeasurementInterval);
            }
        }

        /// <summary>Function that is executed after the measurement task has started.</summary>
        protected override void PostStartMeasurement()
        {
        }

        /// <summary>Disposes the sensor.</summary>
        public override void Dispose() => sensor?.Dispose();

        /// <summary>Stops the sensor.</summary>
        public override void StopSensor()
        {
            sensorTokenSource?.Cancel();
            IsRunning = false;
        }

        /// <summary>
        /// Adjusta the temperature and humidity, measured by a different sensor, in the CCS811 Sensor.
        /// Data is needed for correct measurement.
        /// </summary>
        private void AdjustTemperatureHumidity(EnvironmentMeasurement measurement)
        {
            Debug.WriteLine("+++++ Updating Temperature and Humidity in CCS811 Sensor +++++");


            if (measurement != null)
                sensor.SetEnvironmentData(
                    Temperature.FromDegreesCelsius(measurement.Temperature),
                    RelativeHumidity.FromPercent(measurement.Humidity));
            else
                sensor.SetEnvironmentData(Temperature.FromDegreesCelsius(21.3), RelativeHumidity.FromPercent(42.5));
        }
    }
}