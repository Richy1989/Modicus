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

        private readonly ISettingsManager settingsManager;
        private readonly IMqttManager mqttManager;
        private readonly ITokenManager tokenManager;

        /// <summary>
        /// Creates a new instance of bus device manager.
        /// This instance can handle all sensors connected to a bus of the device.
        /// </summary>
        public BusDeviceManager(ISettingsManager settingsManager, IMqttManager mqttManager, ITokenManager tokenManager)
        {
            //All supported sensors need to be entered here!
            SupportedSensors = new Hashtable
            {
                { "BME280 Sensor", typeof(BME280Sensor) }
            };

            this.ConfiguredSensors = new Hashtable();
            this.settingsManager = settingsManager;
            this.mqttManager = mqttManager;
            this.tokenManager = tokenManager;
            CreateStartAllSensors();
        }

        /// <summary>Adds a new sensor.</summary>
        /// <param name="sensor"></param>
        public void AddSensor(ISensor sensor)
        {
            settingsManager.SensorSettings.AddSensor(sensor);
            sensor.Configure((IPublishMqtt)mqttManager);
            ConfiguredSensors.Add(sensor.Name, sensor);
        }

        /// <summary>Starts the measurement of the sernsor.</summary>
        /// <param name="sensor"></param>
        public void StartSensor(ISensor sensor)
        {
            if (!sensor.IsRunning)
                sensor.StartMeasurement(tokenManager.Token);
        }

        /// <summary>Stops the given sensor .</summary>
        /// <param name="sensor"></param>
        public void StopSensor(ISensor sensor) => sensor.StopSensor();

        /// <summary>Stops the given sensor.</summary>
        /// <param name="sensor"></param>
        public void DeleteSensor(ISensor sensor)
        {
            sensor.StopSensor();
            sensor.Dispose();
            ConfiguredSensors.Remove(sensor.Name);
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
                ConfiguredSensors.Add(baseSensor.Name, baseSensor);

                baseSensor.Configure((IPublishMqtt)mqttManager);
                baseSensor.StartMeasurement(tokenManager.Token);
            }
        }

        /// <summary>Return the cofigured sensor by its name.</summary>
        /// <param name="name"></param>
        public ISensor GetSensorFromName(string name)
        {
            return (ISensor)ConfiguredSensors[name];
        }
    }
}