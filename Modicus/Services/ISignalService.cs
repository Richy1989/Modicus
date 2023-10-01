using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;
using System.Threading;
using nanoFramework.Hardware.Esp32;

namespace Modicus.Services
{
    internal interface ISignalService
    {
        void SignalError(Gpio pin);
    }
}
