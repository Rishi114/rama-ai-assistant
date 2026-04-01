using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Rama.Core;
using Application = System.Windows.Application;

namespace Rama
{
    /// <summary>
    /// System Tray Manager — Makes Rama run in the background like a normal Windows app.
    /// Shows a tray icon, handles notifications, and manages the app lifecycle.
    /// </summary>
    public class SystemTrayManager : IDisposable
    {
        private NotifyIcon _trayIcon = null!;
        private MainWindow? _mainWindow;
        private bool _isMainWindowVisible = true;

        public event Action? OnShowWindow;
        public event Action? OnHideWindow;
        public event Action? OnExit;

        public void Initialize(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            // Create tray icon
            _trayIcon = new NotifyIcon
            {
                Icon = CreateRamaIcon(),
                Text = "Rama — AI Assistant",
                Visible = true
            };

            // Context menu
            var menu = new ContextMenuStrip();

            menu.Items.Add("💬 Open Rama", null, (s, e) => ShowWindow());
            menu.Items.Add("🎤 Toggle Voice", null, (s, e) => ToggleVoice());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("📊 Stats", null, (s, e) => ShowStats());
            menu.Items.Add("🧠 Memory", null, (s, e) => ShowMemory());
            menu.Items.Add("⚙️ Settings", null, (s, e) => ShowSettings());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("❌ Exit", null, (s, e) => ExitApp());

            _trayIcon.ContextMenuStrip = menu;
            _trayIcon.DoubleClick += (s, e) => ToggleWindow();
            _trayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    ToggleWindow();
            };
        }

        public void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info, int durationMs = 3000)
        {
            _trayIcon?.ShowBalloonTip(durationMs, title, message, icon);
        }

        public void ShowWindow()
        {
            if (_mainWindow != null)
            {
                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
                _isMainWindowVisible = true;
                OnShowWindow?.Invoke();
            }
        }

        public void HideWindow()
        {
            if (_mainWindow != null)
            {
                _mainWindow.Hide();
                _isMainWindowVisible = false;
                OnHideWindow?.Invoke();
            }
        }

        public void ToggleWindow()
        {
            if (_isMainWindowVisible)
                HideWindow();
            else
                ShowWindow();
        }

        private void ToggleVoice()
        {
            ShowNotification("Rama", "Voice mode toggled", ToolTipIcon.Info);
        }

        private void ShowStats()
        {
            ShowWindow();
        }

        private void ShowMemory()
        {
            ShowWindow();
        }

        private void ShowSettings()
        {
            ShowWindow();
        }

        private void ExitApp()
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            OnExit?.Invoke();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Create a simple Rama icon programmatically.
        /// </summary>
        private Icon CreateRamaIcon()
        {
            try
            {
                // Try to load custom icon first
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "rama.ico");
                if (File.Exists(iconPath))
                    return new Icon(iconPath);
            }
            catch { }

            // Generate a simple icon
            using var bmp = new Bitmap(16, 16);
            using var g = Graphics.FromImage(bmp);

            // Draw a circle (brain icon)
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            // Blue circle
            using var brush = new SolidBrush(Color.FromArgb(137, 180, 250));
            g.FillEllipse(brush, 1, 1, 14, 14);

            // "R" letter
            using var font = new Font("Arial", 9, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.FromArgb(30, 30, 46));
            var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("R", font, textBrush, 8, 8, format);

            return Icon.FromHandle(bmp.GetHicon());
        }

        public void Dispose()
        {
            _trayIcon?.Dispose();
        }
    }
}
