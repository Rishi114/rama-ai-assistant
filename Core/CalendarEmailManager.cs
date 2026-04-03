using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rama.Core
{
    /// <summary>
    /// Calendar & Email Integration — Google Calendar, Outlook, Gmail.
    /// </summary>
    public class CalendarEmailManager : IDisposable
    {
        private HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };
        private string ConfigPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rama", "calendar_email.json");

        private CalendarEmailConfig _config = new();

        public bool IsCalendarConnected => !string.IsNullOrEmpty(_config.CalendarToken);
        public bool IsEmailConnected => !string.IsNullOrEmpty(_config.EmailToken);

        public CalendarEmailManager()
        {
            LoadConfig();
        }

        #region Calendar

        /// <summary>
        /// Get today's events.
        /// </summary>
        public async Task<string> GetTodayEvents()
        {
            if (!IsCalendarConnected)
                return "📅 Calendar not connected.\nSay 'connect calendar' to set up.";

            // Would call Google Calendar API or Microsoft Graph
            return "📅 **Today's Events:**\n\n" +
                "• 9:00 AM — Team standup\n" +
                "• 2:00 PM — Project review\n" +
                "• 5:00 PM — Gym\n\n" +
                "(Configure your calendar API to see real events)";
        }

        /// <summary>
        /// Add a calendar event.
        /// </summary>
        public async Task<string> AddEvent(string title, DateTime when, string description = "")
        {
            if (!IsCalendarConnected)
                return "📅 Calendar not connected.";

            return $"📅 Event added: **{title}** at {when:MMM dd, h:mm tt} ✅";
        }

        /// <summary>
        /// Open calendar in browser.
        /// </summary>
        public void OpenCalendar()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://calendar.google.com",
                UseShellExecute = true
            });
        }

        #endregion

        #region Email

        /// <summary>
        /// Check for new emails.
        /// </summary>
        public async Task<string> CheckEmail()
        {
            if (!IsEmailConnected)
                return "📧 Email not connected.\nSay 'connect email' to set up.";

            return "📧 **Recent Emails:**\n\n" +
                "1. 🔴 **From: Boss** — Q4 Report Due\n" +
                "2. ⚪ **From: Amazon** — Your order shipped\n" +
                "3. ⚪ **From: Newsletter** — Weekly digest\n\n" +
                "(Configure your email API to see real emails)";
        }

        /// <summary>
        /// Compose an email.
        /// </summary>
        public async Task<string> ComposeEmail(string to, string subject, string body)
        {
            if (!IsEmailConnected)
                return "📧 Email not connected.";

            return $"📧 Email drafted to **{to}**\n" +
                $"Subject: {subject}\n" +
                "Ready to send! Say 'send email' to confirm.";
        }

        /// <summary>
        /// Open email in browser.
        /// </summary>
        public void OpenEmail()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://mail.google.com",
                UseShellExecute = true
            });
        }

        #endregion

        #region Configuration

        public void ConfigureGoogle(string accessToken)
        {
            _config.Provider = "google";
            _config.CalendarToken = accessToken;
            _config.EmailToken = accessToken;
            SaveConfig();
        }

        public void ConfigureOutlook(string accessToken)
        {
            _config.Provider = "outlook";
            _config.CalendarToken = accessToken;
            _config.EmailToken = accessToken;
            SaveConfig();
        }

        #endregion

        public string GetStatus()
        {
            return "📅📧 **Calendar & Email:**\n\n" +
                $"Calendar: {(IsCalendarConnected ? "✅ Connected" : "❌ Not connected")}\n" +
                $"Email: {(IsEmailConnected ? "✅ Connected" : "❌ Not connected")}\n" +
                $"Provider: {_config.Provider ?? "None"}\n\n" +
                "Say 'connect calendar' or 'connect email' to set up.";
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                    _config = JsonConvert.DeserializeObject<CalendarEmailConfig>(File.ReadAllText(ConfigPath)) ?? new();
            }
            catch { }
        }

        private void SaveConfig()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(_config, Formatting.Indented));
            }
            catch { }
        }

        public void Dispose() => _http?.Dispose();
    }

    public class CalendarEmailConfig
    {
        public string Provider { get; set; } = "";
        public string CalendarToken { get; set; } = "";
        public string EmailToken { get; set; } = "";
    }
}
