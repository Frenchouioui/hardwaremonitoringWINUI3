using HardwareMonitorWinUI3.Models;
using HardwareMonitorWinUI3.Hardware;
using HardwareMonitorWinUI3.UI;
using LibreHardwareMonitor.Hardware;
using Xunit;

namespace HardwareMonitorWinUI3.Tests;

public class UIExtensionsTests
{
    [Fact]
    public void GetSensorUnit_Temperature_ReturnsCelsius()
    {
        var unit = SensorType.Temperature.GetSensorUnit();

        Assert.Equal("°C", unit);
    }

    [Fact]
    public void GetSensorUnit_Voltage_ReturnsVolt()
    {
        var unit = SensorType.Voltage.GetSensorUnit();

        Assert.Equal("V", unit);
    }

    [Fact]
    public void GetSensorUnit_Load_ReturnsPercent()
    {
        var unit = SensorType.Load.GetSensorUnit();

        Assert.Equal("%", unit);
    }

    [Fact]
    public void GetSensorUnit_Fan_ReturnsRPM()
    {
        var unit = SensorType.Fan.GetSensorUnit();

        Assert.Equal("RPM", unit);
    }

    [Fact]
    public void GetSensorUnit_Clock_ReturnsMHz()
    {
        var unit = SensorType.Clock.GetSensorUnit();

        Assert.Equal("MHz", unit);
    }

    [Fact]
    public void GetSensorUnit_Power_ReturnsWatt()
    {
        var unit = SensorType.Power.GetSensorUnit();

        Assert.Equal("W", unit);
    }

    [Fact]
    public void GetSensorUnit_Data_ReturnsGB()
    {
        var unit = SensorType.Data.GetSensorUnit();

        Assert.Equal("GB", unit);
    }

    [Fact]
    public void GetSensorUnit_Throughput_ReturnsBs()
    {
        var unit = SensorType.Throughput.GetSensorUnit();

        Assert.Equal("B/s", unit);
    }

    [Fact]
    public void GetSensorPrecision_Temperature_ReturnsF1()
    {
        var precision = SensorType.Temperature.GetSensorPrecision();

        Assert.Equal("F1", precision);
    }

    [Fact]
    public void GetSensorPrecision_Voltage_ReturnsF3()
    {
        var precision = SensorType.Voltage.GetSensorPrecision();

        Assert.Equal("F3", precision);
    }

    [Fact]
    public void GetSensorPrecision_Clock_ReturnsF0()
    {
        var precision = SensorType.Clock.GetSensorPrecision();

        Assert.Equal("F0", precision);
    }

    [Fact]
    public void GetSensorPrecision_Fan_ReturnsF0()
    {
        var precision = SensorType.Fan.GetSensorPrecision();

        Assert.Equal("F0", precision);
    }

    [Fact]
    public void GetBackdropDisplayName_Acrylic_ReturnsAcrylic()
    {
        var result = UIExtensions.GetBackdropDisplayName(BackdropStyle.Acrylic);

        Assert.Contains("Acrylic", result);
    }

    [Fact]
    public void GetBackdropDisplayName_Mica_ReturnsMica()
    {
        var result = UIExtensions.GetBackdropDisplayName(BackdropStyle.Mica);

        Assert.Contains("Mica", result);
    }

    [Fact]
    public void GetBackdropDisplayName_MicaAlt_ReturnsMicaAlt()
    {
        var result = UIExtensions.GetBackdropDisplayName(BackdropStyle.MicaAlt);

        Assert.Contains("Mica Alt", result);
    }

    [Theory]
    [InlineData(SensorType.Temperature, "°C", "F1")]
    [InlineData(SensorType.Voltage, "V", "F3")]
    [InlineData(SensorType.Clock, "MHz", "F0")]
    [InlineData(SensorType.Load, "%", "F1")]
    [InlineData(SensorType.Fan, "RPM", "F0")]
    [InlineData(SensorType.Power, "W", "F1")]
    [InlineData(SensorType.Data, "GB", "F1")]
    [InlineData(SensorType.Throughput, "B/s", "F1")]
    public void SensorType_HasCorrectUnitAndPrecision(SensorType type, string expectedUnit, string expectedPrecision)
    {
        Assert.Equal(expectedUnit, type.GetSensorUnit());
        Assert.Equal(expectedPrecision, type.GetSensorPrecision());
    }
}
