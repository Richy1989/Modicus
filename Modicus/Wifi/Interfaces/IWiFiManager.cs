namespace Modicus.Wifi.Interfaces
{
    internal interface IWiFiManager
    {
        void Start();
        bool ISoftAP { get; }
    }
}