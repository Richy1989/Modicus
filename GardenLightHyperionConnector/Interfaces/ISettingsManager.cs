using System;
using System.Collections.Generic;
using System.Text;
using Modicus.Settings;

namespace Modicus.Interfaces
{
    internal interface ISettingsManager
    {
        GlobalSettings GlobalSettings { get; }
        void UpdateSettings();
    }
}
