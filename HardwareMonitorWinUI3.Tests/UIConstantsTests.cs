using System;
using HardwareMonitorWinUI3.Models;
using HardwareMonitorWinUI3.UI;
using Xunit;

namespace HardwareMonitorWinUI3.Tests;

public class UIConstantsTests
{
    [Fact]
    public void ApplicationTitle_IsNotEmpty()
    {
        Assert.False(string.IsNullOrEmpty(UIConstants.ApplicationTitle));
    }

    [Theory]
    [InlineData(250, "Ultra")]
    [InlineData(500, "Fast")]
    [InlineData(1000, "Normal")]
    [InlineData(999, "Ultra")]
    [InlineData(0, "Ultra")]
    [InlineData(-1, "Ultra")]
    public void GetActiveSpeedButton_ReturnsCorrectButton(int interval, string expected)
    {
        var result = UIConstants.GetActiveSpeedButton(interval);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetUpsIndicator_FormatsCorrectly()
    {
        var result = UIConstants.GetUpsIndicator(60, 250);

        Assert.Equal("60 UPS (250ms)", result);
    }

    [Fact]
    public void GetInitialUpsIndicator_FormatsCorrectly()
    {
        var result = UIConstants.GetInitialUpsIndicator(500);

        Assert.Equal("0 UPS (500ms)", result);
    }

    [Fact]
    public void GetInitializationMessage_ReturnsNonEmpty()
    {
        var result = UIConstants.GetInitializationMessage();

        Assert.False(string.IsNullOrEmpty(result));
    }

    [Fact]
    public void GetActiveMonitoringMessage_IncludesCounts()
    {
        var result = UIConstants.GetActiveMonitoringMessage(10, 3);

        Assert.Contains("10", result);
        Assert.Contains("3", result);
    }

    [Fact]
    public void GetErrorMessage_IncludesErrorText()
    {
        var result = UIConstants.GetErrorMessage("Test error");

        Assert.Contains("Test error", result);
    }

    [Fact]
    public void Intervals_HaveCorrectOrder()
    {
        Assert.True(UIConstants.UltraInterval < UIConstants.FastInterval);
        Assert.True(UIConstants.FastInterval < UIConstants.NormalInterval);
    }
}
