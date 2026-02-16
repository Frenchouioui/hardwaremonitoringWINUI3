using HardwareMonitorWinUI3.Models;
using Xunit;

namespace HardwareMonitorWinUI3.Tests;

public class AppSettingsTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
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
    public void IsMaximized_DefaultIsFalse()
    {
        var settings = new AppSettings();

        Assert.False(settings.IsMaximized);
    }
}
