using System.Device.Gpio;
using System.Threading;
using Modicus.Manager;

namespace Modicus.Services
{
    internal class SignalService
    {
        public static Thread errorsignal;
        private static bool Running;

        public static void SignalError(int millisecondsDelay)
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