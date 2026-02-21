using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
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

                ViewModel.BackdropChanged += OnBackdropChanged;
                ViewModel.NotifySettingsLoaded();

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

        private void ToggleGroup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: SensorGroup sensorGroup })
            {
                sensorGroup.IsExpanded = !sensorGroup.IsExpanded;
            }
        }

        private void OnBackdropChanged(object? sender, BackdropStyle backdropStyle)
        {
            SafeApplyBackdrop((int)backdropStyle);
        }

        private void HideHardware_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is string hardwareName)
            {
                ViewModel.HideHardwareCommand.Execute(hardwareName);
            }
        }

        private void HardwareCard_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement? element = sender as Border;
            if (element == null) element = sender as Expander;
            
            if (element != null)
            {
                var flyout = FlyoutBase.GetAttachedFlyout(element) as MenuFlyout;
                if (flyout != null)
                {
                    if (flyout.Items.Count > 0 && element.Tag is string hardwareName)
                    {
                        flyout.Items[0].Tag = hardwareName;
                    }
                    flyout.ShowAt(element, e.GetPosition(element));
                }
            }
        }

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T typed)
                    return typed;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private async void ShowHiddenHardware_Click(object sender, RoutedEventArgs e)
        {
            var hiddenItems = ViewModel.GetHiddenHardwareNamesList();
            
            if (hiddenItems.Count == 0)
            {
                var noItemsDialog = new ContentDialog
                {
                    Title = "Hidden Items",
                    Content = "No items are hidden.",
                    CloseButtonText = "Close",
                    XamlRoot = Content.XamlRoot
                };
                await noItemsDialog.ShowAsync();
                return;
            }

            var stackPanel = new StackPanel { Spacing = 8 };
            
            foreach (var itemName in hiddenItems)
            {
                var button = new Button
                {
                    Content = itemName,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Tag = itemName,
                    CornerRadius = new Microsoft.UI.Xaml.CornerRadius(6)
                };
                button.Click += RestoreHiddenItem_Click;
                stackPanel.Children.Add(button);
            }

            var dialog = new ContentDialog
            {
                Title = "Hidden Items",
                Content = new ScrollViewer
                {
                    Content = stackPanel,
                    MaxHeight = 300
                },
                CloseButtonText = "Close",
                XamlRoot = Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void RestoreHiddenItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string hardwareName)
            {
                ViewModel.ShowHardwareCommand.Execute(hardwareName);
            }
        }

        private void RestoreAllHidden_Click(object sender, RoutedEventArgs e)
        {
            var hiddenItems = ViewModel.GetHiddenHardwareNamesList();
            foreach (var itemName in hiddenItems)
            {
                ViewModel.ShowHardwareCommand.Execute(itemName);
            }
        }

        #endregion

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                ViewModel.BackdropChanged -= OnBackdropChanged;
                ViewModel?.Dispose();
            }
        }
    }
}
