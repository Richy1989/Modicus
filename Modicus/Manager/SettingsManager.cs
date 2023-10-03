using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Modicus.Manager.Interfaces;
using Modicus.Settings;
using nanoFramework.Json;

namespace Modicus.Manager
{
    internal class SettingsManager : ISettingsManager
    {
        //Setings file Path (I = Internal Flash Memory)
        private const string FilePath = "I:\\settings.json";

        //Make sure only one thread at the time can modify the settings file
        private ManualResetEvent mreSettings = new(true);

        public GlobalSettings GlobalSettings { get; private set; }

        public SettingsManager() 
        {
            LoadSettings(false);
        }

        public void LoadSettings(bool resetSettings = false)
        {
          //  mreSettings.WaitOne();

            //Delete Settings File
            if (resetSettings)
                DeleteSettingsFile();

            //If not exist -> Create new Setting File
            if (!File.Exists(FilePath))
            {
                CreateNewSettingsFile();
            }
            else
            {
                //Read settings from settings file
                Debug.WriteLine("+++++ Read settings from file +++++");
                FileStream fs2 = new(FilePath, FileMode.Open, FileAccess.ReadWrite);
                byte[] fileContent = new byte[fs2.Length];
                fs2.Read(fileContent, 0, (int)fs2.Length);

                if (fileContent.Length == 0)
                {
                    CreateNewSettingsFile();
                }
                else
                {
                    var settingsText = Encoding.UTF8.GetString(fileContent, 0, (int)fs2.Length);
                    fs2.Dispose();

                    Debug.WriteLine("+++++ Settings Text: +++++");
                    Debug.WriteLine(settingsText);

                    GlobalSettings = (GlobalSettings)JsonConvert.DeserializeObject(settingsText, typeof(GlobalSettings));
                    GlobalSettings.IsFreshInstall = false;
                }
            }
        //    mreSettings.Set();
        }

        private void CreateNewSettingsFile()
        {
            Debug.WriteLine("+++++ Create new Settings File +++++");
            GlobalSettings newSettings = new()
            {
                IsFreshInstall = true
            };
            CreateSettingFile(newSettings);
            GlobalSettings = newSettings;
        }


        //Delete the settings file and create the new one with the actual settings
        //Make sure only one task can enter this function
        public void UpdateSettings()
        {
            mreSettings.WaitOne();
            Debug.WriteLine("+++++ Updating the settings file: +++++");

            if (GlobalSettings != null)
            {
                CreateSettingFile(GlobalSettings);
            }
            Debug.WriteLine("+++++ Settings file updated. +++++");
            mreSettings.Set();
        }

        //Create the settings json file
        private void CreateSettingFile(GlobalSettings settingsFile)
        {
            File.Create(FilePath);
            FileStream fileStream = new(FilePath, FileMode.Open, FileAccess.ReadWrite);
            var newSettingsText = JsonConvert.SerializeObject(settingsFile);

            Debug.WriteLine($"++++ Settings File to create: {newSettingsText}");

            var settingsBuffer = Encoding.UTF8.GetBytes(newSettingsText);
            fileStream.Write(settingsBuffer, 0, settingsBuffer.Length);
            fileStream.Dispose();
        }

        //Delete the settings file 
        private void DeleteSettingsFile()
        {
            Debug.WriteLine("+++++ Deleting Settings File +++++");
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
    }
}