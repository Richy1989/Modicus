using System.Collections;
using Modicus.Manager;
using Modicus.Sensor.Interfaces;
using nanoFramework.Json;

namespace Modicus.Settings
{
    internal class SensorSettings
    {
        private const string filepath = "I:\\sensor_settings.json";
        private readonly SaveLoadFileManager saveLoadFileManager;

        /// <summary>
        /// The sensor list serialized
        /// </summary>
        public IList SensorsStringList { get; set; } = new ArrayList();

        /// <summary>
        /// Creates a new instance of the settings for the Sensors.
        /// This class keeps all the sensor saved and handels the read an write to a file.
        /// </summary>
        public SensorSettings()
        {
            saveLoadFileManager = new SaveLoadFileManager();
        }

        /// <summary>
        /// Saves the settings to a file
        /// </summary>
        public void SaveSettings()
        {
            string sensorSettingString = JsonConvert.SerializeObject(SensorsStringList);
            saveLoadFileManager.CreateSettingFile(filepath, sensorSettingString);
        }

        /// <summary>
        /// Reads the settings from a file
        /// </summary>
        public void LoadSettings()
        {
            string sensorSettingString = saveLoadFileManager.LoadSettings(filepath);
            try
            {
                SensorsStringList = (ArrayList)JsonConvert.DeserializeObject(sensorSettingString, typeof(ArrayList));
            }
            catch
            {
                SensorsStringList = new ArrayList();
            }
        }

        /// <summary>
        /// Adds a new sensor to the collection of sensors
        /// </summary>
        /// <param name="sensor"></param>
        public void AddSensor(ISensor sensor)
        {
            sensor.Type = sensor.GetType().FullName;
            SensorsStringList.Add(JsonSerializer.SerializeObject(sensor));
            SaveSettings();
        }

        /// <summary>
        /// Removes a sensor from the sensor collection
        /// </summary>
        /// <param name="sensor"></param>
        public void RemoveSensor(ISensor sensor)
        {
            SensorsStringList.Remove(sensor);
        }
    }
}