namespace Modicus.Settings
{
    /// <summary>
    /// Class for sensor data configuration.
    /// This class saves its own file since there is problem with deserializing classes which contains lists
    /// </summary>
    internal class SystemSettings
    {
        public bool UseSignalling { get; set; } = true;
        public string InstanceName { get; set; }
        public int SignalGpioPin { get; set; } = 2;
    }
}