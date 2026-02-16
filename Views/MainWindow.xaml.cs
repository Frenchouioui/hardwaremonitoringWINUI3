using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using HardwareMonitorWinUI3.Core;
using HardwareMonitorWinUI3.Models;
using HardwareMonitorWinUI3.Services;
using HardwareMonitorWinUI3.UI;
using HardwareMonitorWinUI3.Shared;

namespace HardwareMonitorWinUI3.Views
{
    public sealed partial class MainWindow : Window, IDisposable
    {
        public AppViewModel ViewModel { get; }
        private bool _uiReady;
        private bool _disposed;

        public MainWindow(
            AppViewModel viewModel,
            IWindowService windowService,
            ISettingsService settingsService)
        {
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            try
            {
                InitializeComponent();

                Title = UIConstants.ApplicationTitle;
                UIExtensions.SetupModernTitleBar(this);

                var backdropIndex = settingsService.Settings.BackdropStyle;
                SafeApplyBackdrop(backdropIndex);
                BackdropSelector.SelectedIndex = backdropIndex;

                ViewModel.NotifySettingsLoaded();

                _ = InitializeHardwareAsync();

                _uiReady = true;
            }
            catch (Exception ex)
            {
                Logger.LogCriticalError("MainWindow constructor", ex);
                UIExtensions.ShowCriticalErrorDialog(ex, Content?.XamlRoot);
            }
        }

        private void SafeApplyBackdrop(int backdropIndex)
        {
            try
            {
                UIExtensions.ApplySelectedBackdrop(this, backdropIndex);
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Failed to apply backdrop {backdropIndex}: {ex.Message}");
            }
        }

        private async Task InitializeHardwareAsync()
        {
            try
            {
                await ViewModel.InitializeAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Logger.LogCriticalError("Hardware initialization", ex);
                ViewModel.SystemStatusText = UIConstants.GetErrorMessage(ex.Message);
            }
        }

        #region Event Handlers

        private void ToggleExpand_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: HardwareNode node })
            {
                node.IsExpanded = !node.IsExpanded;
            }
        }

        private void BackdropSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_uiReady) return;

            if (sender is ComboBox comboBox && comboBox.SelectedIndex >= 0)
            {
                SafeApplyBackdrop(comboBox.SelectedIndex);
                ViewModel?.ChangeBackdropCommand?.Execute(comboBox.SelectedIndex);
            }
        }

        private void ToggleGroup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: SensorGroup sensorGroup })
            {
                sensorGroup.IsExpanded = !sensorGroup.IsExpanded;
            }
        }

        #endregion

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                ViewModel?.Dispose();
            }
        }
    }
}
