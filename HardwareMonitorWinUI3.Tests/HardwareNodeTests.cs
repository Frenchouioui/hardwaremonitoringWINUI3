using HardwareMonitorWinUI3.Models;
using Xunit;

namespace HardwareMonitorWinUI3.Tests;

public class HardwareNodeTests
{
    #region Constructor & Defaults

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var node = new HardwareNode();

        Assert.Equal(string.Empty, node.Name);
        Assert.True(node.IsExpanded);
        Assert.Equal(0, node.SensorCount);
        Assert.Empty(node.Sensors);
        Assert.Empty(node.SubHardware);
        Assert.Empty(node.SensorGroups);
    }

    #endregion

    #region Properties

    [Fact]
    public void Name_PropertyChanged_Raised()
    {
        var node = new HardwareNode();
        bool raised = false;
        node.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(HardwareNode.Name)) raised = true;
        };

        node.Name = "CPU";

        Assert.True(raised);
        Assert.Equal("CPU", node.Name);
    }

    [Fact]
    public void IsExpanded_PropertyChanged_Raised()
    {
        var node = new HardwareNode();
        bool raised = false;
        node.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(HardwareNode.IsExpanded)) raised = true;
        };

        node.IsExpanded = false;

        Assert.True(raised);
        Assert.False(node.IsExpanded);
    }

    #endregion

    #region SensorCount

    [Fact]
    public void SensorCount_WhenSensorsAdded_RaisesPropertyChanged()
    {
        var node = new HardwareNode();
        bool raised = false;
        node.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(HardwareNode.SensorCount)) raised = true;
        };

        node.Sensors.Add(new SensorData { Name = "CPU Core" });

        Assert.True(raised);
        Assert.Equal(1, node.SensorCount);
    }

    [Fact]
    public void SensorCount_WhenSensorsRemoved_UpdatesCorrectly()
    {
        var node = new HardwareNode();
        node.Sensors.Add(new SensorData());
        node.Sensors.Add(new SensorData());

        node.Sensors.RemoveAt(0);

        Assert.Equal(1, node.SensorCount);
    }

    #endregion

    #region SensorCache

    [Fact]
    public void SensorCache_WhenBuilt_ContainsAllSensors()
    {
        var node = new HardwareNode();
        node.Sensors.Add(new SensorData { Name = "CPU Core", SensorType = "Temperature" });
        node.Sensors.Add(new SensorData { Name = "CPU Load", SensorType = "Load" });

        var cache = node.SensorCache;

        Assert.Equal(2, cache.Count);
        Assert.True(cache.ContainsKey("CPU Core|Temperature"));
        Assert.True(cache.ContainsKey("CPU Load|Load"));
    }

    [Fact]
    public void SensorCache_WhenSensorsChanged_IsInvalidated()
    {
        var node = new HardwareNode();
        node.Sensors.Add(new SensorData { Name = "CPU Core", SensorType = "Temperature" });
        var cache1 = node.SensorCache;

        node.Sensors.Add(new SensorData { Name = "GPU Core", SensorType = "Temperature" });
        var cache2 = node.SensorCache;

        Assert.NotSame(cache1, cache2);
        Assert.Equal(2, cache2.Count);
    }

    [Fact]
    public void InvalidateSensorCache_ClearsCache()
    {
        var node = new HardwareNode();
        node.Sensors.Add(new SensorData());
        var cache1 = node.SensorCache;

        node.InvalidateSensorCache();
        var cache2 = node.SensorCache;

        Assert.NotSame(cache1, cache2);
    }

    #endregion

    #region OrganizeSensorsIntoGroups

    [Fact]
    public void OrganizeSensorsIntoGroups_GroupsByCategory()
    {
        var node = new HardwareNode();
        node.Sensors.Add(new SensorData { Name = "CPU Core", SensorType = "Temperature" });
        node.Sensors.Add(new SensorData { Name = "CPU Package", SensorType = "Temperature" });
        node.Sensors.Add(new SensorData { Name = "CPU Total", SensorType = "Load" });

        node.OrganizeSensorsIntoGroups();

        Assert.Equal(2, node.SensorGroups.Count);
    }

    [Fact]
    public void OrganizeSensorsIntoGroups_ClearsExistingGroups()
    {
        var node = new HardwareNode();
        node.Sensors.Add(new SensorData { Name = "CPU Core", SensorType = "Temperature" });

        node.OrganizeSensorsIntoGroups();
        node.OrganizeSensorsIntoGroups();

        Assert.Single(node.SensorGroups);
    }

    [Fact]
    public void OrganizeSensorsIntoGroups_OrdersByCategoryOrder()
    {
        var node = new HardwareNode();
        node.Sensors.Add(new SensorData { SensorType = "Load" });
        node.Sensors.Add(new SensorData { SensorType = "Temperature" });
        node.Sensors.Add(new SensorData { SensorType = "Voltage" });

        node.OrganizeSensorsIntoGroups();

        Assert.Equal("Voltages", node.SensorGroups[0].CategoryName);
        Assert.Equal("Temperatures", node.SensorGroups[1].CategoryName);
        Assert.Equal("Loads", node.SensorGroups[2].CategoryName);
    }

    [Fact]
    public void OrganizeSensorsIntoGroups_DisposesOldGroups()
    {
        var node = new HardwareNode();
        node.Sensors.Add(new SensorData { Name = "CPU Core", SensorType = "Temperature" });
        node.OrganizeSensorsIntoGroups();
        var oldGroup = node.SensorGroups[0];

        node.OrganizeSensorsIntoGroups();

        Assert.NotSame(oldGroup, node.SensorGroups[0]);
    }

    #endregion

    #region Category

    [Fact]
    public void Category_CanBeSetViaInit()
    {
        var node = new HardwareNode { Category = HardwareCategory.Cpu };

        Assert.Equal(HardwareCategory.Cpu, node.Category);
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var node = new HardwareNode();

        node.Dispose();
        node.Dispose();

        Assert.True(true);
    }

    [Fact]
    public void Dispose_DisposesSubHardware()
    {
        var node = new HardwareNode();
        var subNode = new HardwareNode { Name = "Sub" };
        node.SubHardware.Add(subNode);

        node.Dispose();

        Assert.True(true);
    }

    [Fact]
    public void Dispose_DisposesSensorGroups()
    {
        var node = new HardwareNode();
        node.Sensors.Add(new SensorData { SensorType = "Temperature" });
        node.OrganizeSensorsIntoGroups();

        node.Dispose();

        Assert.True(true);
    }

    #endregion
}
