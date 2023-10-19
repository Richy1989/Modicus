namespace Modicus.Settings
{
    public class MqttSettings
    {
        //Default Measurement Interval is 10 Seconds

        public bool ConnectToMqtt { get; set; }
        public string MqttUserName { get; set; }
        public string MqttPassword { get; set; }
        public string MqttClientID { get; set; }
        public string MqttHostName { get; set; }
        public int MqttPort { get; set; } = 1883;
    }
}