using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;
using Microsoft.Extensions.DependencyInjection;
using HardwareMonitorWinUI3.Views;
using HardwareMonitorWinUI3.Hardware;
using HardwareMonitorWinUI3.Core;
using HardwareMonitorWinUI3.Services;
using HardwareMonitorWinUI3.Shared;

namespace HardwareMonitorWinUI3
{
    public partial class App : Application
    {
        private MainWindow? _window;
        private ServiceProvider? _serviceProvider;
        private DispatcherQueue? _dispatcherQueue;

        public App()
        {
            InitializeComponent();
            UnhandledException += OnUnhandledException;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            var services = new ServiceCollection();

            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            services.AddSingleton(_dispatcherQueue);
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<IHardwareService, HardwareService>();
            services.AddTransient<AppViewModel>(sp =>
                new AppViewModel(
                    sp.GetRequiredService<IHardwareService>(),
                    sp.GetRequiredService<ISettingsService>(),
                    action =>
                    {
                        if (_dispatcherQueue == null) return false;
                        return _dispatcherQueue.TryEnqueue(new DispatcherQueueHandler(action));
                    }));

            _serviceProvider = services.BuildServiceProvider();

            var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
            var windowService = _serviceProvider.GetRequiredService<IWindowService>();
            var viewModel = _serviceProvider.GetRequiredService<AppViewModel>();

            _window = new MainWindow(viewModel, windowService, settingsService);
            _window.Closed += OnWindowClosed;

            windowService.RestoreWindowState(_window);

            _window.Activate();
        }

        private void OnWindowClosed(object sender, WindowEventArgs e)
        {
            if (_window != null && _serviceProvider != null)
            {
                var windowService = _serviceProvider.GetService<IWindowService>();
                windowService?.SaveWindowState(_window);
            }
            CleanupResources();
        }

        private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Logger.LogCriticalError("Unhandled exception", e.Exception);
            e.Handled = true;
            CleanupResources();
            Exit();
        }

        private void CleanupResources()
        {
            try
            {
                if (_window != null)
                {
                    _window.Closed -= OnWindowClosed;
                    _window.Dispose();
                    _window = null;
                }

                _serviceProvider?.Dispose();
                _serviceProvider = null;
                _dispatcherQueue = null;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error during cleanup", ex);
            }
        }
    }
}
