using System;
using System.Windows;

namespace Rama
{
    public partial class App : Application
    {
        private SystemTrayManager? _trayManager;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize system tray
            _trayManager = new SystemTrayManager();

            // Create main window
            var mainWindow = new MainWindow();
            _trayManager.Initialize(mainWindow);

            // Handle minimize to tray
            mainWindow.StateChanged += (s, args) =>
            {
                if (mainWindow.WindowState == WindowState.Minimized)
                {
                    mainWindow.Hide();
                    _trayManager.ShowNotification("Rama", "Minimized to tray. Double-click to open.");
                }
            };

            // Handle close to tray (don't exit)
            mainWindow.Closing += (s, args) =>
            {
                args.Cancel = true;
                mainWindow.Hide();
                _trayManager.ShowNotification("Rama", "Running in background. Right-click tray icon to exit.");
            };

            // Show window
            mainWindow.Show();

            // Welcome notification
            _trayManager.ShowNotification("Rama", "Your AI assistant is ready! 🤖");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayManager?.Dispose();
            base.OnExit(e);
        }
    }
}
