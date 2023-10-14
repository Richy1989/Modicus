using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;

namespace Modicus.Helpers.Interfaces
{
    internal interface ISignalService
    {
        public void SignalError();
        public void SignalReboot();
        public void SignalOff();
        public void SignalOn();
    }
}
