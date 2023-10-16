namespace Modicus.OutputDevice.Interface
{
    internal interface IOutputDevice
    {
        /// <summary>Publishes all data from modicus on the output device.</summary>
        /// <param name="state">The state.</param>
        /// <param name="sensorData">The sensor data.</param>
        void PublishAll(string state, string sensorData);
    }
}