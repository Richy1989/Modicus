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
        private CancellationTokenSource source;
        private CancellationToken token;

        public SignalService() 
        {
            
        }

        private void Signal(int millisecondsDelay)
        {
            if(errorsignal != null)
            {
                errorsignal.Abort();
                source?.Cancel();
            }

            source = new CancellationTokenSource();
            token = source.Token;

            var pin = ModicusStartupManager.pin;
            errorsignal = new Thread(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    pin.Write(PinValue.Low);
                    Thread.Sleep(millisecondsDelay);
                    pin.Write(PinValue.High);
                    Thread.Sleep(millisecondsDelay);
                }
            });

            errorsignal.Start();
        }

        public void SignalError()
        {
            if (Running) return;
            Running = true;
            Signal(200);
        }

        public void SignalReboot()
        {
            Signal(1000);
        }
    }
}