using System.Collections.ObjectModel;
using System.ComponentModel;
using Moq;
using HardwareMonitorWinUI3.Core;
using HardwareMonitorWinUI3.Hardware;
using HardwareMonitorWinUI3.Models;
using HardwareMonitorWinUI3.Services;
using HardwareMonitorWinUI3.Shared;
using HardwareMonitorWinUI3.UI;

namespace HardwareMonitorWinUI3.Tests;

public class AppViewModelTests
{
    private readonly Mock<IHardwareService> _mockService;
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly Mock<ILogger> _mockLogger;
    private readonly AppViewModel _viewModel;
    private readonly ObservableCollection<HardwareNode> _serviceNodes;

    public AppViewModelTests()
    {
        _mockService = new Mock<IHardwareService>();
        _mockSettingsService = new Mock<ISettingsService>();
        _mockLogger = new Mock<ILogger>();
        _serviceNodes = new ObservableCollection<HardwareNode>();

        var settings = new AppSettings();
        _mockSettingsService.Setup(s => s.Settings).Returns(settings);
        _mockService.Setup(s => s.HardwareNodes).Returns(_serviceNodes);
        _mockService.Setup(s => s.CurrentInterval).Returns(UIConstants.UltraInterval);
        _mockService.Setup(s => s.IsInitialized).Returns(true);

        _viewModel = new AppViewModel(_mockService.Object, _mockSettingsService.Object, _mockLogger.Object);
    }

    #region Speed Button State

    [Fact]
    public void InitialActiveSpeedButton_IsUltra()
    {
        Assert.Equal("Ultra", _viewModel.ActiveSpeedButton);
        Assert.True(_viewModel.IsUltraActive);
        Assert.False(_viewModel.IsFastActive);
        Assert.False(_viewModel.IsNormalActive);
    }

    [Theory]
    [InlineData(250, "Ultra", true, false, false)]
    [InlineData(500, "Fast", false, true, false)]
    [InlineData(1000, "Normal", false, false, true)]
    public void ChangeRefreshSpeed_UpdatesActiveButton(int interval, string expected, bool ultra, bool fast, bool normal)
    {
        _mockService.Setup(s => s.CurrentInterval).Returns(interval);

        _viewModel.ChangeRefreshSpeed(interval);

        Assert.Equal(expected, _viewModel.ActiveSpeedButton);
        Assert.Equal(ultra, _viewModel.IsUltraActive);
        Assert.Equal(fast, _viewModel.IsFastActive);
        Assert.Equal(normal, _viewModel.IsNormalActive);
    }

    [Fact]
    public void ChangeRefreshSpeed_CallsServiceChangeInterval()
    {
        _viewModel.ChangeRefreshSpeed(500);

        _mockService.Verify(s => s.ChangeInterval(500), Times.Once);
    }

    [Fact]
    public void ChangeRefreshSpeed_RaisesPropertyChanged()
    {
        var changedProperties = new List<string>();
        _viewModel.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        _mockService.Setup(s => s.CurrentInterval).Returns(500);
        _viewModel.ChangeRefreshSpeed(500);

        Assert.Contains(nameof(AppViewModel.ActiveSpeedButton), changedProperties);
        Assert.Contains(nameof(AppViewModel.IsUltraActive), changedProperties);
        Assert.Contains(nameof(AppViewModel.IsFastActive), changedProperties);
        Assert.Contains(nameof(AppViewModel.IsNormalActive), changedProperties);
    }

    #endregion

    #region ResetAllMinMax

    [Fact]
    public void ResetAllMinMax_ResetsAllSensors()
    {
        var sensor1 = new SensorData { Value = "50.0°C" };
        sensor1.UpdateMinMax(30f, "°C");
        sensor1.UpdateMinMax(80f, "°C");

        var sensor2 = new SensorData { Value = "1200MHz" };
        sensor2.UpdateMinMax(800f, "MHz", "F0");
        sensor2.UpdateMinMax(1500f, "MHz", "F0");

        var node = new HardwareNode { Name = "CPU", Category = HardwareCategory.Cpu };
        node.Sensors.Add(sensor1);
        node.Sensors.Add(sensor2);
        _serviceNodes.Add(node);

        _viewModel.ResetAllMinMax();

        Assert.Equal("Min: N/A", sensor1.MinValue);
        Assert.Equal("Max: N/A", sensor1.MaxValue);
        Assert.Equal("Min: N/A", sensor2.MinValue);
        Assert.Equal("Max: N/A", sensor2.MaxValue);
    }

    [Fact]
    public void ResetAllMinMax_ResetsSubHardwareSensors()
    {
        var subSensor = new SensorData { Value = "45.0°C" };
        subSensor.UpdateMinMax(20f, "°C");

        var subNode = new HardwareNode { Name = "Sub", Category = HardwareCategory.Motherboard };
        subNode.Sensors.Add(subSensor);

        var mainNode = new HardwareNode { Name = "Main", Category = HardwareCategory.Motherboard };
        mainNode.SubHardware.Add(subNode);
        _serviceNodes.Add(mainNode);

        _viewModel.ResetAllMinMax();

        Assert.Equal("Min: N/A", subSensor.MinValue);
        Assert.Equal("Max: N/A", subSensor.MaxValue);
    }

    #endregion

    #region Filter Logic

    [Fact]
    public void ShowCPU_False_FiltersCpuNodes()
    {
        var cpuNode = new HardwareNode { Name = "CPU", Category = HardwareCategory.Cpu };
        var gpuNode = new HardwareNode { Name = "GPU", Category = HardwareCategory.Gpu };
        _serviceNodes.Add(cpuNode);
        _serviceNodes.Add(gpuNode);

        _viewModel.ShowCPU = false;

        Assert.DoesNotContain(cpuNode, _viewModel.HardwareNodes);
        Assert.Contains(gpuNode, _viewModel.HardwareNodes);
    }

    [Fact]
    public void ShowGPU_False_FiltersGpuNodes()
    {
        var cpuNode = new HardwareNode { Name = "CPU", Category = HardwareCategory.Cpu };
        var gpuNode = new HardwareNode { Name = "GPU", Category = HardwareCategory.Gpu };
        _serviceNodes.Add(cpuNode);
        _serviceNodes.Add(gpuNode);

        _viewModel.ShowGPU = false;

        Assert.Contains(cpuNode, _viewModel.HardwareNodes);
        Assert.DoesNotContain(gpuNode, _viewModel.HardwareNodes);
    }

    [Fact]
    public void Filter_Reenabling_RestoresNode()
    {
        var cpuNode = new HardwareNode { Name = "CPU", Category = HardwareCategory.Cpu };
        _serviceNodes.Add(cpuNode);

        _viewModel.ShowCPU = false;
        Assert.Empty(_viewModel.HardwareNodes);

        _viewModel.ShowCPU = true;
        Assert.Single(_viewModel.HardwareNodes);
        Assert.Contains(cpuNode, _viewModel.HardwareNodes);
    }

    [Theory]
    [InlineData(nameof(AppViewModel.ShowStorage), HardwareCategory.Storage)]
    [InlineData(nameof(AppViewModel.ShowMemory), HardwareCategory.Memory)]
    [InlineData(nameof(AppViewModel.ShowNetwork), HardwareCategory.Network)]
    [InlineData(nameof(AppViewModel.ShowController), HardwareCategory.Controller)]
    [InlineData(nameof(AppViewModel.ShowMotherboard), HardwareCategory.Motherboard)]
    public void Filter_EachCategory_Works(string propertyName, HardwareCategory category)
    {
        var node = new HardwareNode { Name = "Test", Category = category };
        _serviceNodes.Add(node);

        // Use reflection to set the filter property to false
        var prop = typeof(AppViewModel).GetProperty(propertyName)!;
        prop.SetValue(_viewModel, false);

        Assert.DoesNotContain(node, _viewModel.HardwareNodes);

        prop.SetValue(_viewModel, true);
        Assert.Contains(node, _viewModel.HardwareNodes);
    }

    #endregion

    #region Property Change Notifications

    [Theory]
    [InlineData(nameof(AppViewModel.ShowCPU))]
    [InlineData(nameof(AppViewModel.ShowGPU))]
    [InlineData(nameof(AppViewModel.ShowMotherboard))]
    [InlineData(nameof(AppViewModel.ShowStorage))]
    [InlineData(nameof(AppViewModel.ShowMemory))]
    [InlineData(nameof(AppViewModel.ShowNetwork))]
    [InlineData(nameof(AppViewModel.ShowController))]
    public void FilterProperties_RaisePropertyChanged(string propertyName)
    {
        var raised = false;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == propertyName) raised = true;
        };

        var prop = typeof(AppViewModel).GetProperty(propertyName)!;
        prop.SetValue(_viewModel, false);

        Assert.True(raised);
    }

    [Fact]
    public void SystemStatusText_RaisesPropertyChanged()
    {
        var raised = false;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(AppViewModel.SystemStatusText)) raised = true;
        };

        _viewModel.SystemStatusText = "Test";

        Assert.True(raised);
        Assert.Equal("Test", _viewModel.SystemStatusText);
    }

    #endregion

    #region BackdropIndicator

    [Fact]
    public void SetBackdropIndicator_UpdatesProperty()
    {
        _viewModel.SetBackdropIndicator("• Acrylic");

        Assert.Equal("• Acrylic", _viewModel.BackdropIndicator);
    }

    [Fact]
    public void SetBackdropIndicator_NullThrows()
    {
        Assert.Throws<ArgumentNullException>(() => _viewModel.SetBackdropIndicator(null!));
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_UnsubscribesEvents()
    {
        _viewModel.Dispose();

        _mockService.VerifyRemove(s => s.TimerTick -= It.IsAny<EventHandler>(), Times.Once);
        _mockService.VerifyRemove(s => s.UpsUpdated -= It.IsAny<EventHandler<int>>(), Times.Once);
    }

    #endregion
}
