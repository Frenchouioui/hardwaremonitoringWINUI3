// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// Copyright (c) 2024 HardwareMonitorWinUI3 Contributors

using System;
using Microsoft.UI.Xaml;
using HardwareMonitorWinUI3.Views;
using HardwareMonitorWinUI3.Shared;

namespace HardwareMonitorWinUI3
{
    /// <summary>
    /// Point d'entrée de l'application WinUI 3
    /// </summary>
    public partial class App : Application
    {
        private MainWindow? _window;

        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += OnUnhandledException;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Closed += OnWindowClosed;
            _window.Activate();
        }

        private void OnWindowClosed(object sender, WindowEventArgs e)
        {
            CleanupResources();
        }

        private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Logger.LogCriticalError("Unhandled exception", e.Exception);
            CleanupResources();
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
            }
            catch (Exception ex)
            {
                Logger.LogError("Error during cleanup", ex);
            }
        }
    }
}
