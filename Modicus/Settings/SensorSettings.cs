using System.Collections;
using System.Diagnostics;
using System.Threading;
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

        //Make sure only one thread at the time can work with the mqtt service
        private readonly ManualResetEvent mre = new(true);

        /// <summary>The sensor list serialized.</summary>
        public IList SensorsStringList
        {
            get
            {
                return LoadSettings();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorSettings"/> class.
        /// This class keeps all the sensor saved and handels the read an write actions to the settings file.
        /// </summary>
        public SensorSettings()
        {
            saveLoadFileManager = new SaveLoadFileManager();
        }

        /// <summary>Reads the settings from a file.</summary>
        public IList LoadSettings()
        {
            mre.WaitOne();

            IList localSensorsStringList;
            string sensorSettingString = saveLoadFileManager.LoadSettings(filepath);
            Debug.WriteLine("+++ Read Configured Sensors +++");
            Debug.WriteLine(sensorSettingString);
            try
            {
                localSensorsStringList = (ArrayList)JsonConvert.DeserializeObject(sensorSettingString, typeof(ArrayList));
            }
            catch
            {
                Debug.WriteLine($"No sensors configured, creating new sensor string list");
                localSensorsStringList = new ArrayList();
            }

            mre.Set();
            return localSensorsStringList;
        }

        /// <summary>Saves the sensors to a json file.</summary>
        /// <param name="configuredSensors">The configured sensors.</param>
        public void SaveSensors(IDictionary configuredSensors)
        {
            mre.WaitOne();
            ICollection sensors = configuredSensors.Values;
            
            IList sensorList = new ArrayList();
            foreach (ISensor item in sensors)
            {
                item.Type = item.GetType().FullName;
                sensorList.Add(JsonSerializer.SerializeObject(item));
            }

            string sensorSettingString = JsonConvert.SerializeObject(sensorList);
            sensorList.Clear();

            Debug.Write(sensorSettingString);

            saveLoadFileManager.CreateSettingFile(filepath, sensorSettingString);
            mre.Set();
        }
    }
}