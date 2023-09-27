
namespace GardenLightHyperionConnector.Settings
{
    public class MqttSettings
    {
        //Default Measurement Interval is 5 Minutes
        public int SendInterval { get; set; } = 1000;//1000 * 60 * 5;
        public bool ConnectToMqtt { get; set; }
        public string MqttUserName { get; set; }
        public string MqttPassword { get; set; }
        public string MqttClientID { get; set; }
        public string MqttHostName { get; set; }
    }
}
