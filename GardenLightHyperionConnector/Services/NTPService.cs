using nanoFramework.Networking;

namespace Modicus.Services
{
    public class NTPService
    {
        public NTPService()
        {
            Sntp.Server2 = "bevtime1.metrologie.at";
            Sntp.Server1 = "ts1.univie.ac.at";
            Sntp.Start();
            Sntp.UpdateNow();
        }
    }
}
