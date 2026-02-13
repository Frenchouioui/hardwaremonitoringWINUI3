using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using HardwareMonitorWinUI3.Core;
using HardwareMonitorWinUI3.Hardware;
using HardwareMonitorWinUI3.Models;
using HardwareMonitorWinUI3.UI;
using HardwareMonitorWinUI3.Shared;

namespace HardwareMonitorWinUI3.Views
{
    public sealed partial class MainWindow : Window, IDisposable
    {
        public AppViewModel ViewModel { get; private set; } = null!;
        private bool _uiReady;
        private bool _disposed;

        public MainWindow()
        {
            try
            {
                this.InitializeComponent();

                if (!DiagnosticHelper.CheckAdministratorRights())
                {
                    Logger.LogWarning("L'application n'a pas les droits administrateur");
                }

                var hardwareService = new HardwareService(this.DispatcherQueue);
                ViewModel = new AppViewModel(hardwareService, this.DispatcherQueue);

                SetupUI();

                this.SetupModernTitleBar();
                this.SetMicaBackdrop(Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt);

                InitializeHardwareAsync();

                _uiReady = true;
            }
            catch (Exception ex)
            {
                Logger.LogCriticalError("MainWindow constructor", ex);
                UIExtensions.ShowCriticalErrorDialog(ex, this.Content?.XamlRoot);
            }
        }

        private void SetupUI()
        {
            this.SetupModernInterface(BackdropSelector, HardwareIcon,
                UltraButton, RapideButton, NormalButton, ResetButton, HardwareDiagButton);

            UltraButton.Tag = ViewModel.UltraInterval.ToString();
            RapideButton.Tag = ViewModel.RapideInterval.ToString();
            NormalButton.Tag = ViewModel.NormalInterval.ToString();

            UIExtensions.UpdateSpeedButtonVisualState(UltraButton, RapideButton, NormalButton, ViewModel.ActiveSpeedButton);
        }

        private async void InitializeHardwareAsync()
        {
            try
            {
                await ViewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                Logger.LogCriticalError("initialisation hardware", ex);
                ViewModel.SystemStatusText = UIConstants.GetErrorMessage(ex.Message);
            }
        }

        #region Event Handlers

        private void SpeedButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag?.ToString() is string tagValue)
            {
                ViewModel.ChangeSpeedCommand.Execute(tagValue);
                UIExtensions.UpdateSpeedButtonVisualState(UltraButton, RapideButton, NormalButton, ViewModel.ActiveSpeedButton);
            }
        }

        private void ToggleExpand_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: HardwareNode node })
            {
                node.IsExpanded = !node.IsExpanded;
            }
        }

        private void HardwareDiagButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.RunDiagnosticCommand?.Execute(null);
        }

        private void BackdropSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_uiReady) return;

            if (sender is ComboBox comboBox && comboBox.SelectedIndex >= 0)
            {
                this.ApplySelectedBackdrop(comboBox.SelectedIndex);
                ViewModel?.ChangeBackdropCommand?.Execute(comboBox.SelectedIndex);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.ResetMinMaxCommand?.Execute(null);
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
                ViewModel = null!;
            }
        }
    }
}
