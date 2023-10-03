using System;
using System.Collections;
using Modicus.Sensor;
using Modicus.Sensor.Interfaces;
using nanoFramework.Json;

namespace Modicus.Settings
{
    internal class SensorSettings
    {
        /// <summary>
        /// The actual sensor, private so we do not serialize them
        /// </summary>
        private IDictionary Sensors { get; set; } = new Hashtable();

        /// <summary>
        /// The Sensors as string for serialization
        /// </summary>
        public IList SensorsString
        {
            get
            {
                IList values = new ArrayList();

                foreach (var item in Sensors)
                {
                    ((ISensor)item).Type = item.GetType().FullName;
                    values.Add(JsonSerializer.SerializeObject(item));
                }

                return values;
            }
            set
            {
                Sensors.Clear();
                foreach (var item in value)
                {
                    ISensor baseSensor = (ISensor)JsonConvert.DeserializeObject((string)item, typeof(BaseSensor));
                    baseSensor = (ISensor)JsonConvert.DeserializeObject((string)item, Type.GetType(baseSensor.Type));
                    Sensors.Add(baseSensor.Name, baseSensor);
                }
            }
        }

        public void AddSensor(ISensor sensor)
        {
            Sensors.Add(sensor.Name, sensor);
        }

        public void RemoveSensor()
        {
        }

        public ISensor GetSensor(string name)
        {
            return (ISensor)Sensors[name];
        }

        public IDictionary GetAllConfiguredSensors()
        {
            return Sensors;
        }
    }
}