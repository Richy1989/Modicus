using System.Threading;
using Modicus.Manager.Interfaces;
using Modicus.Services.Interfaces;
using Modicus.Wifi.Interfaces;
using nanoFramework.Networking;

namespace Modicus.Services
{
    internal class NTPService : INtpService
    {
        readonly Thread ntpStarter;
        
        /// <summary>Creates a new Service for NTP Updates.</summary>
        /// <param name="wiFiManager"></param>
        /// <param name="tokenManager"></param>
        public NTPService(IWiFiManager wiFiManager, ITokenManager tokenManager)
        {
            Sntp.Server1 = "at.pool.ntp.org";
            Sntp.Server2 = "ts1.univie.ac.at";

            //ntpStarter = new Thread(() =>
            //{
            //    while (!tokenManager.Token.IsCancellationRequested)
            //    {
            //        while (!wiFiManager.IsConnected && !wiFiManager.ISoftAP && !tokenManager.Token.IsCancellationRequested)
            //        {
            //            Thread.Sleep(5000);
            //        }

            //        Sntp.Start();

            //        while (wiFiManager.IsConnected && !tokenManager.Token.IsCancellationRequested)
            //        {
            //            Thread.Sleep(1000);
            //            Sntp.UpdateNow();
            //        }
            //        Thread.Sleep(1000);
            //    }
            //});
        }

        public void Start()
        {
            // ntpStarter.Start();
            Sntp.Start();
        }
    }
}