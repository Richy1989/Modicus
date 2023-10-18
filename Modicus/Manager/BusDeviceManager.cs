using System;
using System.Collections;
using Modicus.Manager.Interfaces;
using Modicus.MQTT.Interfaces;
using Modicus.Sensor;
using Modicus.Sensor.Interfaces;
using nanoFramework.Json;

namespace Modicus.Manager
{
    internal class BusDeviceManager : IBusDeviceManager
    {
        public IDictionary ConfiguredSensors { get; }
        public IDictionary SupportedSensors { get; }
        public IDictionary OutputManagers { get; }

        private readonly ISettingsManager settingsManager;
        private readonly ITokenManager tokenManager;
        private readonly IOutputManager outputManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BusDeviceManager"/> class.
        /// This instance can handle all sensors connected to a bus of the device.
        /// </summary>
        /// <param name="settingsManager">The settings manager.</param>
        /// <param name="mqttManager">The MQTT manager.</param>
        /// <param name="tokenManager">The token manager.</param>
        public BusDeviceManager(
            ISettingsManager settingsManager,
            IOutputManager outputManager,
            ITokenManager tokenManager)
        {
            //All supported sensors need to be entered here!
            SupportedSensors = new Hashtable
            {
                { "BME280 Sensor", typeof(BME280Sensor) },
                { "CCS811 Sensor", typeof(CCS811GasSensor) }
            };

            this.ConfiguredSensors = new Hashtable();
            this.settingsManager = settingsManager;
            this.tokenManager = tokenManager;
            this.outputManager = outputManager;
            CreateStartAllSensors();
        }

        /// <summary>Adds a new sensor.</summary>
        /// <param name="sensor"></param>
        public void AddSensor(ISensor sensor)
        {
            if (ConfiguredSensors.Contains(sensor.Name))
                return;

            //settingsManager.SensorSettings.AddSensor(sensor);

            sensor.Configure();
            ConfiguredSensors.Add(sensor.Name, sensor);
            settingsManager.SensorSettings.SaveSensors(ConfiguredSensors);
        }

        /// <summary>Starts the measurement of the sernsor.</summary>
        /// <param name="sensor"></param>
        public void StartSensor(ISensor sensor)
        {
            if (sensor != null && !sensor.IsRunning)
            {
                sensor.MeasurementAvailable += Sensor_MeasurementAvailable;
                sensor.StartMeasurement(tokenManager.Token);
            }
        }

        /// <summary>Handles the MeasurementAvailable event of the Sensor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs.MeasurementAvailableEventArgs"/> instance containing the event data.</param>
        private void Sensor_MeasurementAvailable(object sender, EventArgs.MeasurementAvailableEventArgs e)
        {
            outputManager.AddMeasurementData(e.Data);

            //Check if all sensors is depended on received mesurement, if so inject it.
            foreach (ISensor sensor in ConfiguredSensors.Values)
            {
                var dependedMeasurementList = sensor.DependsOnMeasurement();

                if (dependedMeasurementList != null)
                {
                    foreach (Type dependedType in dependedMeasurementList)
                    {
                        if (dependedType == e.Data.GetType())
                        {
                            sensor.InjectDependedMeasurement(e.Data);
                        }
                    }
                }
            }
        }

        /// <summary>Stops the given sensor .</summary>
        /// <param name="sensor"></param>
        public void StopSensor(ISensor sensor)
        {
            if (sensor == null) return;

            sensor.MeasurementAvailable -= Sensor_MeasurementAvailable;
            sensor.StopSensor();
        }

        /// <summary>Stops the given sensor.</summary>
        /// <param name="sensor"></param>
        public void DeleteSensor(ISensor sensor)
        {
            if (sensor == null) return;

            StopSensor(sensor);
            sensor.Dispose();
            ConfiguredSensors.Remove(sensor.Name);
            settingsManager.SensorSettings.SaveSensors(ConfiguredSensors);
            outputManager.PurgeMeasurementData(sensor.Measurement);
        }

        /// <summary>Returns a sensor.</summary>
        /// <param name="name"></param>
        public ISensor GetSensor(string name) => (ISensor)ConfiguredSensors[name];

        /// <summary>Creates and starts all saved sensors.</summary>
        private void CreateStartAllSensors()
        {
            ConfiguredSensors.Clear();

            var sensorString = settingsManager.SensorSettings.SensorsStringList;

            foreach (var item in sensorString)
            {
                ISensor baseSensor = (ISensor)JsonConvert.DeserializeObject((string)item, typeof(BaseSensor));
                baseSensor = (ISensor)JsonConvert.DeserializeObject((string)item, Type.GetType(baseSensor.Type));

                if (!ConfiguredSensors.Contains(baseSensor.Name))
                {
                    ConfiguredSensors.Add(baseSensor.Name, baseSensor);

                    baseSensor.Configure();
                    StartSensor(baseSensor);
                }
            }
        }

        /// <summary>Return the cofigured sensor by its name.</summary>
        /// <param name="name"></param>
        public ISensor GetSensorFromName(string name)
        {
            if (name == null) return null;
            return (ISensor)ConfiguredSensors[name];
        }
    }
}