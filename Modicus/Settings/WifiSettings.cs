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
        public bool StaticAddress { get; set; }
        public bool StartinAPMode { get; set; }
    }
}