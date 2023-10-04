namespace Modicus.Wifi.Interfaces
{
    internal interface IWiFiManager
    {
        bool IsConnected { get; }
        void Start();
        bool ISoftAP { get; }
    }
}