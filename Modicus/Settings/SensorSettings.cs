using System.Collections;
using System.Diagnostics;
using Modicus.Manager;
using Modicus.Sensor.Interfaces;
using nanoFramework.Json;

namespace Modicus.Settings
{
    /// <summary>
    /// Class for sensor data configuration.
    /// This class saves its own file since there is problem with deserializing classes which contains lists
    /// </summary>
    internal class SensorSettings
    {
        private const string filepath = "I:\\sensor_settings.json";
        private readonly SaveLoadFileManager saveLoadFileManager;

        /// <summary>
        /// The sensor list serialized.
        /// </summary>
        public IList SensorsStringList { get; set; } = new ArrayList();

        /// <summary>
        /// Creates a new instance of the settings for the ConfiguredSensors.
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
                Debug.WriteLine($"No sonsors configured, creating new sensor string list");
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