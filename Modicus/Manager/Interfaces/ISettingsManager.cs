using Modicus.Settings;

namespace Modicus.Manager.Interfaces
{
    internal interface ISettingsManager
    {
        public CommandSettings CommandSettings { get; }
        GlobalSettings GlobalSettings { get; }
        SensorSettings SensorSettings { get; }

        void UpdateSettings();

        void LoadSettings(bool resetSettings = false);
    }
}