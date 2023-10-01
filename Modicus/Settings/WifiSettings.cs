namespace Modicus.Settings
{
    public class WifiSettings
    {
        public bool ConnectToWifi { get; set; }
        public string Ssid { get; set; }
        public string Password { get; set; }
        public string IP { get; set; }
        public string NetworkMask { get; set; }
        public string DefaultGateway { get; set; }
        public bool UseDHCP { get; set; } = true;

        //First startup should always be in Access Point Mode
        public bool StartInAPMode { get; set; } = true;
    }
}