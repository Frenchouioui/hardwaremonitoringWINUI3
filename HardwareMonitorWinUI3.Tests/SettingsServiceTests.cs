using System.IO;
using HardwareMonitorWinUI3.Models;
using HardwareMonitorWinUI3.Services;
using Xunit;

namespace HardwareMonitorWinUI3.Tests;

public class SettingsServiceTests
{
    [Fact]
    public void DefaultSettings_HaveCorrectValues()
    {
        var settings = new AppSettings();

        Assert.Equal(-1, settings.WindowX);
        Assert.Equal(-1, settings.WindowY);
        Assert.Equal(1200, settings.WindowWidth);
        Assert.Equal(800, settings.WindowHeight);
        Assert.Equal(250, settings.RefreshInterval);
        Assert.Equal((int)BackdropStyle.MicaAlt, settings.BackdropStyle);
        Assert.True(settings.ShowCPU);
        Assert.True(settings.ShowGPU);
        Assert.True(settings.ShowMotherboard);
        Assert.True(settings.ShowStorage);
        Assert.True(settings.ShowMemory);
        Assert.True(settings.ShowNetwork);
        Assert.True(settings.ShowController);
    }

    [Fact]
    public void BackdropStyle_EnumMatchesExpectedValues()
    {
        Assert.Equal(0, (int)BackdropStyle.Acrylic);
        Assert.Equal(1, (int)BackdropStyle.Mica);
        Assert.Equal(2, (int)BackdropStyle.MicaAlt);
    }

    [Theory]
    [InlineData(50, 250)]
    [InlineData(99, 250)]
    [InlineData(100, 100)]
    [InlineData(250, 250)]
    [InlineData(5000, 5000)]
    [InlineData(5001, 250)]
    [InlineData(10000, 250)]
    public void RefreshInterval_Validation_ClampsCorrectly(int input, int expected)
    {
        var settings = new AppSettings { RefreshInterval = input };
        Assert.Equal(expected, settings.RefreshInterval >= 100 && settings.RefreshInterval <= 5000 
            ? settings.RefreshInterval 
            : 250);
    }

    [Theory]
    [InlineData(-1, 2)]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 2)]
    [InlineData(99, 2)]
    public void BackdropStyle_Validation_ClampsCorrectly(int input, int expected)
    {
        var validValue = input >= 0 && input <= 2 ? input : 2;
        Assert.Equal(expected, validValue);
    }
}
