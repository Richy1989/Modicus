using Modicus.Services.Interfaces;
using nanoFramework.Networking;

namespace Modicus.Services
{
    internal class NTPService : INtpService
    {
        /// <summary>Creates a new Service for NTP Updates.</summary>
        /// <param name="wiFiManager"></param>
        /// <param name="tokenManager"></param>
        public NTPService()
        {
           
        }

        public void Start()
        {
            Sntp.Server1 = "at.pool.ntp.org";
            //Sntp.Server2 = "ts1.univie.ac.at";
            // ntpStarter.Start();
            Sntp.Start();
        }
    }
}