using System;
using System.Collections.Generic;
using System.Text;
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

        public abstract void PublishAll(string state, string sensorData);
    }
}
