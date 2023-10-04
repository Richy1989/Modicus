using Modicus.Settings;

namespace Modicus.Manager.Interfaces
{
    internal interface ISettingsManager
    {
        GlobalSettings GlobalSettings { get; }
        SensorSettings SensorSettings { get; }

        void UpdateSettings();

        void LoadSettings(bool resetSettings = false);
    }
}