using System.Diagnostics;

namespace Rama.Skills
{
    /// <summary>
    /// Launches Windows applications and executables by name.
    /// Supports common app aliases (e.g., "chrome" → Google Chrome) and can
    /// launch apps found via PATH or known install locations.
    /// </summary>
    public class AppLauncherSkill : SkillBase
    {
        public override string Name => "App Launcher";

        public override string Description => "Open applications and programs on your computer";

        public override string[] Triggers => new[]
        {
            "open", "launch", "start", "run", "execute"
        };

        /// <summary>
        /// Maps common app names to their executable names or full paths.
        /// Expanded as users use different apps (via learning).
        /// </summary>
        private static readonly Dictionary<string, string> KnownApps = new(StringComparer.OrdinalIgnoreCase)
        {
            { "notepad", "notepad.exe" },
            { "calculator", "calc.exe" },
            { "calc", "calc.exe" },
            { "paint", "mspaint.exe" },
            { "explorer", "explorer.exe" },
            { "file explorer", "explorer.exe" },
            { "cmd", "cmd.exe" },
            { "command prompt", "cmd.exe" },
            { "terminal", "wt.exe" },
            { "powershell", "powershell.exe" },
            { "control panel", "control.exe" },
            { "task manager", "taskmgr.exe" },
            { "settings", "ms-settings:" },
            { "snipping tool", "snippingtool.exe" },
            { "wordpad", "wordpad.exe" },
            { "charmap", "charmap.exe" },
            { "registry", "regedit.exe" },
            { "registry editor", "regedit.exe" },
            { "device manager", "devmgmt.msc" },
            { "disk manager", "diskmgmt.msc" },
            { "system info", "msinfo32.exe" },
            { "chrome", "chrome" },
            { "google chrome", "chrome" },
            { "firefox", "firefox" },
            { "edge", "msedge" },
            { "microsoft edge", "msedge" },
            { "vs code", "code" },
            { "visual studio code", "code" },
            { "visual studio", "devenv" },
            { "spotify", "spotify" },
            { "discord", "discord" },
            { "slack", "slack" },
            { "steam", "steam" },
            { "vlc", "vlc" },
            { "7zip", "7zFM" },
            { "winrar", "winrar" },
        };

        public override bool CanHandle(string input)
        {
            var lower = input.ToLowerInvariant();
            return Triggers.Any(t =>
            {
                // Match "open X" but not "open a file" (handled by FileManager)
                if (lower.StartsWith(t + " ") || lower == t)
                {
                    var after = lower.Substring(t.Length).Trim();
                    // Don't match file management commands
                    var fileKeywords = new[] { "file", "folder", "directory", "document", "the file" };
                    return !fileKeywords.Any(f => after == f || after.StartsWith(f + " "));
                }
                return false;
            });
        }

        public override Task<string> ExecuteAsync(string input, Core.Memory memory)
        {
            var appName = ExtractAfterTrigger(input).Trim();

            if (string.IsNullOrWhiteSpace(appName))
                return Task.FromResult("What would you like me to open? Give me an app name, " +
                    "like \"open notepad\" or \"launch chrome\".");

            // Handle "open settings" with ms-settings protocol
            if (appName.Equals("settings", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Process.Start(new ProcessStartInfo("ms-settings:") { UseShellExecute = true });
                    return Task.FromResult("Opening Windows Settings for you! ⚙️");
                }
                catch
                {
                    return Task.FromResult("I tried to open Settings but something went wrong. " +
                        "You can open it manually from the Start menu.");
                }
            }

            // Check known app aliases
            string executable;
            if (KnownApps.TryGetValue(appName, out var knownExe))
            {
                executable = knownExe;
            }
            else
            {
                executable = appName;
            }

            // Try to launch the application
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    UseShellExecute = true
                };

                Process.Start(startInfo);

                // Remember this app for learning
                memory.SetPreference($"last_opened_{appName.ToLowerInvariant()}",
                    DateTime.UtcNow.ToString("O"));

                return Task.FromResult($"Launching **{appName}** for you! 🚀");
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // App not found — try with .exe extension
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = executable + ".exe",
                        UseShellExecute = true
                    });
                    memory.SetPreference($"last_opened_{appName.ToLowerInvariant()}",
                        DateTime.UtcNow.ToString("O"));
                    return Task.FromResult($"Launching **{appName}**! 🚀");
                }
                catch
                {
                    return Task.FromResult(
                        $"I couldn't find an app called **\"{appName}\"**. " +
                        "Try the full name or check if it's installed.\n\n" +
                        "Some apps I know: " + string.Join(", ",
                            KnownApps.Keys.Take(10)) + "...");
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult($"Failed to launch **{appName}**: {ex.Message}\n\n" +
                    "Make sure the app is installed and try again.");
            }
        }
    }
}
