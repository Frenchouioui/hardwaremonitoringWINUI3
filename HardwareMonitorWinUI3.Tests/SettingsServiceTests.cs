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
        Assert.Equal(BackdropStyle.MicaAlt, settings.BackdropStyle);
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
    [InlineData(-1, BackdropStyle.MicaAlt)]
    [InlineData(0, BackdropStyle.Acrylic)]
    [InlineData(1, BackdropStyle.Mica)]
    [InlineData(2, BackdropStyle.MicaAlt)]
    [InlineData(3, BackdropStyle.MicaAlt)]
    [InlineData(99, BackdropStyle.MicaAlt)]
    public void BackdropStyle_Validation_ClampsCorrectly(int input, BackdropStyle expected)
    {
        var validValue = Enum.IsDefined(typeof(BackdropStyle), input) ? (BackdropStyle)input : BackdropStyle.MicaAlt;
        Assert.Equal(expected, validValue);
    }
}
