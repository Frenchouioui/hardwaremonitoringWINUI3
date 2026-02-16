using System.Globalization;
using HardwareMonitorWinUI3.Models;
using Xunit;

namespace HardwareMonitorWinUI3.Tests;

public class SensorDataTests
{
    public SensorDataTests()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    #region Constructor & Defaults

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var sensor = new SensorData();

        Assert.Equal(string.Empty, sensor.Name);
        Assert.Equal(string.Empty, sensor.Icon);
        Assert.Equal(string.Empty, sensor.Value);
        Assert.Equal("Min: N/A", sensor.MinValue);
        Assert.Equal("Max: N/A", sensor.MaxValue);
        Assert.Equal(string.Empty, sensor.SensorType);
    }

    #endregion

    #region SensorCategory

    [Theory]
    [InlineData("Voltage", "Voltages")]
    [InlineData("Clock", "Clocks")]
    [InlineData("Temperature", "Temperatures")]
    [InlineData("Load", "Loads")]
    [InlineData("Fan", "Fans")]
    [InlineData("Flow", "Flows")]
    [InlineData("Control", "Controls")]
    [InlineData("Level", "Levels")]
    [InlineData("Factor", "Factors")]
    [InlineData("Power", "Powers")]
    [InlineData("Data", "Data")]
    [InlineData("SmallData", "Small Data")]
    [InlineData("Frequency", "Frequencies")]
    [InlineData("Throughput", "Throughput")]
    [InlineData("Current", "Current")]
    [InlineData("Unknown", "Others")]
    public void SensorCategory_ReturnsCorrectCategory(string sensorType, string expectedCategory)
    {
        var sensor = new SensorData { SensorType = sensorType };

        Assert.Equal(expectedCategory, sensor.SensorCategory);
    }

    #endregion

    #region CategoryIcon

    [Fact]
    public void CategoryIcon_Temperature_ReturnsCorrectGlyph()
    {
        var sensor = new SensorData { SensorType = "Temperature" };

        Assert.Equal("\uE9CA", sensor.CategoryIcon);
    }

    [Fact]
    public void CategoryIcon_Load_ReturnsCorrectGlyph()
    {
        var sensor = new SensorData { SensorType = "Load" };

        Assert.Equal("\uE9D9", sensor.CategoryIcon);
    }

    [Fact]
    public void CategoryIcon_Unknown_ReturnsDefaultGlyph()
    {
        var sensor = new SensorData { SensorType = "Unknown" };

        Assert.Equal("\uE950", sensor.CategoryIcon);
    }

    #endregion

    #region UpdateMinMax

    [Fact]
    public void UpdateMinMax_FirstValue_SetsBothMinAndMax()
    {
        var sensor = new SensorData();

        sensor.UpdateMinMax(50.5f, "°C", "F1");

        Assert.StartsWith("Min: 50.5", sensor.MinValue);
        Assert.StartsWith("Max: 50.5", sensor.MaxValue);
        Assert.EndsWith("°C", sensor.MinValue);
        Assert.EndsWith("°C", sensor.MaxValue);
    }

    [Fact]
    public void UpdateMinMax_LowerValue_UpdatesMinOnly()
    {
        var sensor = new SensorData();
        sensor.UpdateMinMax(50f, "°C");

        sensor.UpdateMinMax(30f, "°C");

        Assert.StartsWith("Min: 30", sensor.MinValue);
        Assert.StartsWith("Max: 50", sensor.MaxValue);
    }

    [Fact]
    public void UpdateMinMax_HigherValue_UpdatesMaxOnly()
    {
        var sensor = new SensorData();
        sensor.UpdateMinMax(50f, "°C");

        sensor.UpdateMinMax(80f, "°C");

        Assert.StartsWith("Min: 50", sensor.MinValue);
        Assert.StartsWith("Max: 80", sensor.MaxValue);
    }

    [Fact]
    public void UpdateMinMax_NegativeThroughput_Ignored()
    {
        var sensor = new SensorData();
        sensor.UpdateMinMax(100f, "MB/s");

        sensor.UpdateMinMax(-50f, "MB/s");

        Assert.StartsWith("Min: 100", sensor.MinValue);
        Assert.StartsWith("Max: 100", sensor.MaxValue);
    }

    [Fact]
    public void UpdateMinMax_NegativeTemperature_Accepted()
    {
        var sensor = new SensorData();

        sensor.UpdateMinMax(-10f, "°C");

        Assert.StartsWith("Min: -10", sensor.MinValue);
        Assert.StartsWith("Max: -10", sensor.MaxValue);
    }

    [Fact]
    public void UpdateMinMax_NullUnit_ThrowsArgumentNullException()
    {
        var sensor = new SensorData();

        Assert.Throws<ArgumentNullException>(() => sensor.UpdateMinMax(50f, null!));
    }

    [Fact]
    public void UpdateMinMax_NullPrecision_ThrowsArgumentNullException()
    {
        var sensor = new SensorData();

        Assert.Throws<ArgumentNullException>(() => sensor.UpdateMinMax(50f, "°C", null!));
    }

    [Fact]
    public void UpdateMinMax_CustomPrecision_AppliesCorrectly()
    {
        var sensor = new SensorData();

        sensor.UpdateMinMax(50.123f, "V", "F3");

        Assert.StartsWith("Min: 50.123", sensor.MinValue);
        Assert.Contains("V", sensor.MinValue);
    }

    #endregion

    #region ResetMinMax

    [Fact]
    public void ResetMinMax_ClearsAllValues()
    {
        var sensor = new SensorData();
        sensor.UpdateMinMax(50f, "°C");
        sensor.UpdateMinMax(80f, "°C");

        sensor.ResetMinMax();

        Assert.Equal("Min: N/A", sensor.MinValue);
        Assert.Equal("Max: N/A", sensor.MaxValue);
    }

    #endregion

    #region PropertyChangeNotifications

    [Fact]
    public void Name_PropertyChanged_Raised()
    {
        var sensor = new SensorData();
        bool raised = false;
        sensor.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SensorData.Name)) raised = true;
        };

        sensor.Name = "Test";

        Assert.True(raised);
    }

    [Fact]
    public void Value_PropertyChanged_Raised()
    {
        var sensor = new SensorData();
        bool raised = false;
        sensor.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SensorData.Value)) raised = true;
        };

        sensor.Value = "50°C";

        Assert.True(raised);
    }

    [Fact]
    public void MinValue_PropertyChanged_Raised()
    {
        var sensor = new SensorData();
        bool raised = false;
        sensor.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SensorData.MinValue)) raised = true;
        };

        sensor.UpdateMinMax(50f, "°C");

        Assert.True(raised);
    }

    [Fact]
    public void SensorType_PropertyChanged_RaisesCategoryAndIconNotifications()
    {
        var sensor = new SensorData();
        var changedProperties = new System.Collections.Generic.List<string>();
        sensor.PropertyChanged += (_, e) =>
        {
            changedProperties.Add(e.PropertyName ?? "");
        };

        sensor.SensorType = "Temperature";

        Assert.Contains(nameof(SensorData.SensorType), changedProperties);
        Assert.Contains(nameof(SensorData.SensorCategory), changedProperties);
        Assert.Contains(nameof(SensorData.CategoryIcon), changedProperties);
    }

    #endregion

    #region CachedProperties

    [Fact]
    public void SensorCategory_IsCached()
    {
        var sensor = new SensorData { SensorType = "Temperature" };

        var category1 = sensor.SensorCategory;
        var category2 = sensor.SensorCategory;

        Assert.Same(category1, category2);
    }

    [Fact]
    public void CategoryIcon_IsCached()
    {
        var sensor = new SensorData { SensorType = "Load" };

        var icon1 = sensor.CategoryIcon;
        var icon2 = sensor.CategoryIcon;

        Assert.Same(icon1, icon2);
    }

    #endregion
}
