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
        private readonly ISettingsManager settingsManager;
        private readonly IMqttManager mqttManager;
        private readonly ITokenManager tokenManager;
        private readonly IDictionary ConfiguredSensors;
        public IList SupportedSensors { get; }

        /// <summary>
        /// Creates a new instance of bus device manager.
        /// This instance can handle all sensors connected to a bus o the device.
        /// </summary>
        public BusDeviceManager(ISettingsManager settingsManager, IMqttManager mqttManager, ITokenManager tokenManager)
        {
            this.SupportedSensors = new ArrayList
            {
               typeof(BME280Sensor)
            };

            this.ConfiguredSensors = new Hashtable();
            this.settingsManager = settingsManager;
            this.mqttManager = mqttManager;
            this.tokenManager = tokenManager;
            CreateStartAllSensors();
        }

        /// <summary>
        /// Adds a new sensor.
        /// </summary>
        /// <param name="sensor"></param>
        public void AddSensor(ISensor sensor)
        {
            settingsManager.SensorSettings.AddSensor(sensor);
            sensor.Configure((IPublishMqtt)mqttManager);
        }

        /// <summary>
        /// Starts the measurement of the sernsor.
        /// </summary>
        /// <param name="sensor"></param>
        public void StartSensor(ISensor sensor)=> sensor.StartMeasurement(tokenManager.Token);

        /// <summary>
        /// Returns a sensor.
        /// </summary>
        /// <param name="name"></param>
        public ISensor GetSensor(string name)=> (ISensor)ConfiguredSensors[name];

        /// <summary>
        /// Creates and starts all saved sensors.
        /// </summary>
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
    }
}