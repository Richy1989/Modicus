using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Modicus.Manager
{
    internal class SaveLoadFileManager //: ISettingsManager
    {
        //Make sure only one thread at the time can modify the settings file
        private readonly ManualResetEvent mreSettings = new(true);

        /// <summary>Reads the given settings file as string from a file.</summary>
        /// <param name="filePath"></param>
        /// <param name="resetSettings"></param>
        public string LoadSettings(string filePath, bool resetSettings = false)
        {
            mreSettings.WaitOne();

            string setting = "";

            //Delete Settings File
            if (resetSettings)
                DeleteSettingsFile(filePath);

            //If not exist -> Create new Setting File
            if (!File.Exists(filePath))
            {
                CreateSettingFile(filePath, setting);
            }
            else
            {
                //Read settings from settings file
                Debug.WriteLine("+++++ Read settings from file +++++");
                FileStream fs2 = new(filePath, FileMode.Open, FileAccess.ReadWrite);

                //  GlobalSettings = (GlobalSettings)JsonConvert.DeserializeObject(fs2, typeof(GlobalSettings));

                byte[] fileContent = new byte[fs2.Length];
                fs2.Read(fileContent, 0, (int)fs2.Length);

                var settingsText = Encoding.UTF8.GetString(fileContent, 0, (int)fs2.Length);

                fs2.Close();
                fs2.Dispose();

                setting = settingsText;
            }
            mreSettings.Set();
            return setting;
        }

        //Create the settings json file
        public void CreateSettingFile(string filePath, string settings)
        {
            File.Create(filePath);
            FileStream fileStream = new(filePath, FileMode.Open, FileAccess.ReadWrite);
            // var newSettingsText = JsonConvert.SerializeObject(settings);

            Debug.WriteLine($"++++ Settings File to create: {settings}");

            var settingsBuffer = Encoding.UTF8.GetBytes(settings);
            fileStream.Write(settingsBuffer, 0, settingsBuffer.Length);
            fileStream.Close();
            fileStream.Dispose();
        }

        //Delete the settings file
        private void DeleteSettingsFile(string filePath)
        {
            Debug.WriteLine("+++++ Deleting Settings File +++++");
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}