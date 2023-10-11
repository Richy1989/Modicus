namespace Modicus.Settings
{
    /// <summary>
    /// Command Settings 
    /// </summary>
    public class CommandSettings
    {
        public string MeasurementIntervalTopic { get; } = "MeasurementInterval";
        public string MqttClientIdTopic { get; } = "MqttClientId";
        public string MqttSendIntervalTopic { get; } = "MqttSendInterval";
        public string MqttOnOffTopic { get; } = "MqttOnOff";
        public string SensorOnOffTopic { get; } = "SensorOnOff";
        public string RebootControllerTopic { get; } = "Reboot";
        public string CreateI2CSensor { get; } = "CreateI2CSensor";
    }
}