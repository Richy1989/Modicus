using System;
using System.Collections.Generic;
using System.Text;

namespace Modicus.Helpers.Interfaces
{
    internal interface ISignalService
    {
        public void SignalError();
        public void SignalReboot();
    }
}
