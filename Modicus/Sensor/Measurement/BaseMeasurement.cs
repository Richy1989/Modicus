using System;
using System.Collections.Generic;
using System.Text;

namespace Modicus.Sensor.Measurement
{
    internal abstract class BaseMeasurement
    {
        internal string MeasurmentCategory { get; set; }
        internal abstract BaseMeasurement Clone();
    }
}
