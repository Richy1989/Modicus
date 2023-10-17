﻿namespace Modicus.Sensor.Measurement
{
    internal abstract class BaseMeasurement
    {
        /// <summary>Initializes a new instance of the <see cref="BaseMeasurement"/> class.</summary>
        /// <param name="measurmentCategory">The measurment category.</param>
        public BaseMeasurement(string measurmentCategory)
        {
            MeasurmentCategory = measurmentCategory;
        }

        /// <summary>Gets or sets the measurment category.</summary>
        /// <value>The measurment category.</value>
        internal string MeasurmentCategory { get; set; }

        /// <summary>Clones this instance.</summary>
        internal abstract BaseMeasurement Clone();
    }
}