namespace HardwareMonitorWinUI3.Models
{
    /// <summary>
    /// Represents sensor data with value tracking and min/max statistics.
    /// </summary>
    public interface ISensorData
    {
        /// <summary>
        /// Gets or sets the sensor name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the icon glyph for the sensor.
        /// </summary>
        string Icon { get; set; }

        /// <summary>
        /// Gets or sets the formatted value string.
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Gets or sets the minimum value string.
        /// </summary>
        string MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value string.
        /// </summary>
        string MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the sensor type identifier.
        /// </summary>
        string SensorType { get; set; }

        /// <summary>
        /// Gets the sensor category group name.
        /// </summary>
        string SensorCategory { get; }

        /// <summary>
        /// Gets the icon glyph for this sensor category.
        /// </summary>
        string CategoryIcon { get; }

        /// <summary>
        /// Updates the min/max values with the current value.
        /// </summary>
        /// <param name="currentValue">The current sensor value.</param>
        /// <param name="unit">The unit string.</param>
        /// <param name="precision">The format precision string.</param>
        void UpdateMinMax(float currentValue, string unit, string precision = "F1");

        /// <summary>
        /// Resets the min/max tracking values.
        /// </summary>
        void ResetMinMax();
    }
}
