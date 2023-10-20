using Modicus.Manager.Interfaces;
using Modicus.OutputDevice.Interface;

namespace Modicus.OutputDevice
{
    internal abstract class BaseOutputDevice : IOutputDevice
    {
        protected readonly IOutputManager outputManager;

        /// <summary>Initializes a new instance of the <see cref="BaseOutputDevice"/> class.</summary>
        /// <param name="outputManager">The output manager.</param>
        public BaseOutputDevice(IOutputManager outputManager)
        {
            this.outputManager = outputManager;
        }

        /// <summary>Publishes all gived data to all defined output devices.</summary>
        /// <param name="state">The state.</param>
        /// <param name="sensorData">The sensor data.</param>
        public abstract void PublishAll(string state, string sensorData);
    }
}