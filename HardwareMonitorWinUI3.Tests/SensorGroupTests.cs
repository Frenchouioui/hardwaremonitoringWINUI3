using HardwareMonitorWinUI3.Models;
using Xunit;

namespace HardwareMonitorWinUI3.Tests;

public class SensorGroupTests
{
    #region Constructor & Defaults

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var group = new SensorGroup();

        Assert.Equal(string.Empty, group.CategoryName);
        Assert.Equal(string.Empty, group.CategoryIcon);
        Assert.True(group.IsExpanded);
        Assert.Equal(0, group.SensorCount);
        Assert.Empty(group.Sensors);
    }

    #endregion

    #region Properties

    [Fact]
    public void CategoryName_PropertyChanged_Raised()
    {
        var group = new SensorGroup();
        bool raised = false;
        group.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SensorGroup.CategoryName)) raised = true;
        };

        group.CategoryName = "Temperatures";

        Assert.True(raised);
        Assert.Equal("Temperatures", group.CategoryName);
    }

    [Fact]
    public void CategoryIcon_PropertyChanged_Raised()
    {
        var group = new SensorGroup();
        bool raised = false;
        group.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SensorGroup.CategoryIcon)) raised = true;
        };

        group.CategoryIcon = "\uE9CA";

        Assert.True(raised);
    }

    [Fact]
    public void IsExpanded_PropertyChanged_Raised()
    {
        var group = new SensorGroup();
        bool raised = false;
        group.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SensorGroup.IsExpanded)) raised = true;
        };

        group.IsExpanded = false;

        Assert.True(raised);
        Assert.False(group.IsExpanded);
    }

    #endregion

    #region SensorCount

    [Fact]
    public void SensorCount_WhenSensorsAdded_RaisesPropertyChanged()
    {
        var group = new SensorGroup();
        bool raised = false;
        group.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SensorGroup.SensorCount)) raised = true;
        };

        group.Sensors.Add(new SensorData());

        Assert.True(raised);
        Assert.Equal(1, group.SensorCount);
    }

    [Fact]
    public void SensorCount_MultipleSensors_ReturnsCorrectCount()
    {
        var group = new SensorGroup();
        group.Sensors.Add(new SensorData());
        group.Sensors.Add(new SensorData());
        group.Sensors.Add(new SensorData());

        Assert.Equal(3, group.SensorCount);
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_CanBeCalledWithoutException()
    {
        var group = new SensorGroup();
        group.Sensors.Add(new SensorData());

        group.Dispose();

        Assert.True(true);
    }

    #endregion
}
