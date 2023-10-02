using System.Device.Gpio;
using System.Threading;
using Modicus.Helpers.Interfaces;
using Modicus.Manager;

namespace Modicus.Helpers
{
    internal class SignalService : ISignalService
    {
        public Thread errorsignal;
        private bool Running;

        public void Signal(int millisecondsDelay)
        {
            if (Running) return;
            var pin = ModicusStartupManager.pin;
            errorsignal = new Thread(() =>
            {
                while (true)
                {
                    Running = true;
                    pin.Write(PinValue.Low);
                    Thread.Sleep(millisecondsDelay);
                    pin.Write(PinValue.High);
                    Thread.Sleep(millisecondsDelay);
                }
            });
            errorsignal.Start();
        }
    }
}