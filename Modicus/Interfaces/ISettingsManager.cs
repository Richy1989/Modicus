using Modicus.Settings;

namespace Modicus.Interfaces
{
    internal interface ISettingsManager
    {
        GlobalSettings GlobalSettings { get; }

        void UpdateSettings();

        void LoadSettings(bool resetSettings = false);
    }
}