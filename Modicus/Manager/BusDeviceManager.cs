using System.Collections;
using Modicus.Manager.Interfaces;
using Modicus.MQTT.Interfaces;
using Modicus.Sensor;
using Modicus.Sensor.Interfaces;

namespace Modicus.Manager
{
    internal class BusDeviceManager : IBusDeviceManager
    {
        private readonly ISettingsManager settingsManager;
        private readonly IMqttManager mqttManager;
        private readonly ITokenManager tokenManager;

        /// <summary>
        /// Creates a new instance of bus device manager
        /// </summary>
        public BusDeviceManager(ISettingsManager settingsManager, IMqttManager mqttManager, ITokenManager tokenManager)
        {
            this.settingsManager = settingsManager;
            this.mqttManager = mqttManager;
            this.tokenManager = tokenManager;
            CreateStartAllSensors();
        }

        //public void AddSensor(ISensor sensorData)
        //{
        //    Type sensorTyp = (Type)sensors[sensorData.Name];

        //    if (sensorTyp != null)
        //    {
        //        ISensor device = (ISensor)Activator.CreateInstance(sensorTyp);
        //        device.Configure(sensorData);
        //    }
        //}

        public void AddSensor(ISensor sensor)
        {
            settingsManager.GlobalSettings.SensorSettings.AddSensor(sensor);
            sensor.Configure((IPublishMqtt)mqttManager);
            
        }

        private void CreateStartAllSensors()
        {
            var configuredSensors = settingsManager.GlobalSettings.SensorSettings.GetAllConfiguredSensors();
            foreach (ISensor sensorData in configuredSensors)
            {
                sensorData.Configure((IPublishMqtt)mqttManager);
                sensorData.StartMeasurement(tokenManager.Token);
            }
        }
    }
}