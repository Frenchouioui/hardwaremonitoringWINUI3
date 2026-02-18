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
        private readonly ILogger _logger;
        private bool _uiReady;
        private bool _isBackdropChanging;
        private bool _disposed;

        public MainWindow(
            AppViewModel viewModel,
            IWindowService windowService,
            ISettingsService settingsService,
            ILogger logger)
        {
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            try
            {
                InitializeComponent();

                Title = UIConstants.ApplicationTitle;
                UIExtensions.SetupModernTitleBar(this);

                var backdropIndex = (int)settingsService.Settings.BackdropStyle;
                SafeApplyBackdrop(backdropIndex);
                BackdropSelector.SelectedIndex = backdropIndex;

                ViewModel.NotifySettingsLoaded();

                _uiReady = true;

                _ = InitializeHardwareAsync();
            }
            catch (Exception ex)
            {
                _logger.LogCriticalError("MainWindow constructor", ex);
                _ = ShowCriticalErrorDialogAsync(ex);
            }
        }

        private async Task ShowCriticalErrorDialogAsync(Exception ex)
        {
            await UIExtensions.ShowCriticalErrorDialog(ex, Content?.XamlRoot, _logger);
        }

        private void SafeApplyBackdrop(int backdropIndex)
        {
            try
            {
                UIExtensions.ApplySelectedBackdrop(this, (BackdropStyle)backdropIndex);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to apply backdrop {backdropIndex}: {ex.Message}");
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
                _logger.LogCriticalError("Hardware initialization", ex);
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
            if (!_uiReady || _isBackdropChanging) return;

            if (sender is ComboBox comboBox && comboBox.SelectedIndex >= 0)
            {
                _isBackdropChanging = true;
                comboBox.IsEnabled = false;
                
                try
                {
                    SafeApplyBackdrop(comboBox.SelectedIndex);
                    ViewModel?.ChangeBackdropCommand?.Execute(comboBox.SelectedIndex);
                }
                finally
                {
                    _isBackdropChanging = false;
                    comboBox.IsEnabled = true;
                }
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
