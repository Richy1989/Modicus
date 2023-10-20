using Modicus.Services.Interfaces;
using nanoFramework.Networking;

namespace Modicus.Services
{
    internal class NTPService : INtpService
    {
        /// <summary>Initializes a new instance of the <see cref="NTPService"/> class.</summary>
        public NTPService()
        { }

        /// <summary>Starts the NTP service.</summary>
        public void Start()
        {
            Sntp.Server1 = "at.pool.ntp.org";
            //Sntp.Server2 = "ts1.univie.ac.at";
            // ntpStarter.Start();
            Sntp.Start();
        }
    }
}