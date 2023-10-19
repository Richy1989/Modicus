using System.Device.Gpio;
using System.Threading;
using Modicus.Helpers.Interfaces;
using Modicus.Manager.Interfaces;
using nanoFramework.Hardware.Esp32;

namespace Modicus.Helpers
{
    internal class SignalService : ISignalService
    {
        private readonly GpioController controller;
        private readonly ISettingsManager settingManager;
        private readonly GpioPin pin;

        private Thread errorsignal;
        private bool Running;

        private CancellationTokenSource source;
        private CancellationToken token;

        /// <summary>Initializes a new instance of the <see cref="SignalService"/> class.</summary>
        /// <param name="settingManager">The setting manager.</param>
        public SignalService(ISettingsManager settingManager, GpioController controller)
        {
            this.controller = controller;
            this.settingManager = settingManager;
            if (!settingManager.GlobalSettings.SystemSettings.UseSignalling) return;

            pin = controller.OpenPin(Gpio.IO02, PinMode.Output);
        }

        /// <summary>Signals at the defined GPIO output.</summary>
        /// <param name="millisecondsDelay">The milliseconds delay.</param>
        private void Signal(int millisecondsDelay)
        {
            if (!settingManager.GlobalSettings.SystemSettings.UseSignalling) return;

            if (errorsignal != null)
            {
                errorsignal.Abort();
                source?.Cancel();
            }

            source = new CancellationTokenSource();
            token = source.Token;
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

        /// <summary>Signals an Error at the defined GPIO output.</summary>
        public void SignalError()
        {
            if (Running) return;
            Running = true;
            Signal(200);
        }

        /// <summary>Signals a "Reboot Needed" at the defined GPIO output.</summary>
        public void SignalReboot()
        {
            Signal(1000);
        }

        /// <summary>Turn the signal GPIO off.</summary>
        public void SignalOff()
        {
            if (!settingManager.GlobalSettings.SystemSettings.UseSignalling) return;
            pin.Write(PinValue.Low);
        }

        /// <summary>Turn the signal GPIO on.</summary>
        public void SignalOn()
        {
            if (!settingManager.GlobalSettings.SystemSettings.UseSignalling) return;
            pin.Write(PinValue.High);
        }
    }
}