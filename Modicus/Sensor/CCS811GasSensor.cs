﻿using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Ccs811;
using Modicus.MQTT.Interfaces;
using UnitsNet;

namespace Modicus.Sensor
{
    internal class CCS811GasSensor : BaseI2cSensor
    {
        private Ccs811Sensor sensor;
        private IPublishMqtt publishMqtt;
        private CancellationToken token;

        public double eCO2 { get; set; }
        public double eTVOC { get; set; }
        public double Current { get; set; }
        public double ADC { get; set; }

        public CCS811GasSensor()
        {
        }

        public override void Configure(IPublishMqtt publisher)
        {
            base.Configure(publisher);

            this.publishMqtt = publisher;

            sensor = new Ccs811Sensor(GetI2cDevice())
            {
                OperationMode = OperationMode.ConstantPower1Second
            };
        }

        public override void StartMeasurement(CancellationToken token)
        {
            this.token = token;

            sensorTokenSource = new CancellationTokenSource();
            sensorToken = sensorTokenSource.Token;

            sensorThread = new Thread(() =>
            {
                while (!token.IsCancellationRequested && !sensorToken.IsCancellationRequested)
                {
                    while (!sensor.IsDataReady && !token.IsCancellationRequested && !sensorToken.IsCancellationRequested)
                    {
                        Thread.Sleep(100);
                    }

                    var success = sensor.TryReadGasData(out VolumeConcentration eCO2, out VolumeConcentration eTVOC, out ElectricCurrent curr, out int adc);

                    if (success)
                    {
                        Debug.WriteLine($"Success: {success}, eCO2: {eCO2.PartsPerMillion} ppm, eTVOC: {eTVOC.PartsPerBillion} ppb, Current: {curr.Microamperes} µA, ADC: {adc} = {adc * 1.65 / 1023} V.");

                        this.eCO2 = eCO2.PartsPerMillion;
                        this.eTVOC = eTVOC.PartsPerBillion;
                        this.Current = curr.Microamperes;
                        this.ADC = adc * 1.65 / 1023;

                        if (publishMqtt != null)
                        {
                            publishMqtt.MainMqttMessage.Environment.TotalVolatileCompound = this.eCO2;
                            publishMqtt.MainMqttMessage.Environment.TotalVolatileOrganicCompound = this.eTVOC;
                        }
                    }
                    Thread.Sleep(MeasurementInterval);
                }
                IsRunning = false;
            });

            Thread adjustThread = new(new ThreadStart(AdjustTemperatureHumidity));
            adjustThread.Start();

            Thread.Sleep(500);

            sensorThread.Start();
        }

        /// <summary>Disposes the sensor.</summary>
        public override void Dispose() => sensor?.Dispose();

        /// <summary>Stops the sensor.</summary>
        public override void StopSensor()
        {
            sensorTokenSource?.Cancel();

            if (publishMqtt != null)
                publishMqtt.MainMqttMessage.Environment = null;

            IsRunning = false;
        }

        private void AdjustTemperatureHumidity()
        {
            while (!token.IsCancellationRequested && !sensorTokenSource.IsCancellationRequested)
            {
                Debug.WriteLine("+++++ Updating Temperature and Humidity in CC811 Sensor +++++");

                if (publishMqtt.MainMqttMessage.Environment != null)
                    sensor.SetEnvironmentData(Temperature.FromDegreesCelsius(publishMqtt.MainMqttMessage.Environment.Temperature), RelativeHumidity.FromPercent(publishMqtt.MainMqttMessage.Environment.Humidity));
                else
                    sensor.SetEnvironmentData(Temperature.FromDegreesCelsius(21.3), RelativeHumidity.FromPercent(42.5));

                Thread.Sleep(TimeSpan.FromSeconds(60));
            }
        }
    }
}