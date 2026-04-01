using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rama.Core
{
    /// <summary>
    /// Application Controller — Access and control ANY Windows application.
    /// Can launch, interact with, and automate any software.
    /// Learns how to use software by asking the user, then remembers forever.
    /// </summary>
    public class AppController : IDisposable
    {
        private string DataDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rama", "AppController");

        private string SoftwareDbPath => Path.Combine(DataDir, "software.json");
        private string ProceduresPath => Path.Combine(DataDir, "procedures.json");

        private List<SoftwareInfo> _software = new();
        private List<LearnedProcedure> _procedures = new();

        // Win32 API for window control
        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] private static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)] 
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public AppController()
        {
            Directory.CreateDirectory(DataDir);
            LoadAll();
            ScanInstalledSoftware();
        }

        #region Launch & Control

        /// <summary>
        /// Launch any application.
        /// </summary>
        public async Task<string> LaunchApp(string appName)
        {
            try
            {
                // Check known apps
                var known = FindSoftware(appName);
                if (known != null)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = known.ExecutablePath,
                        UseShellExecute = true
                    });
                    return $"✅ Launched **{known.Name}**!";
                }

                // Try common names
                string? path = FindExecutable(appName);
                if (path != null)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                    return $"✅ Opened **{appName}**!";
                }

                // Try shell execute
                Process.Start(new ProcessStartInfo
                {
                    FileName = appName,
                    UseShellExecute = true
                });
                return $"✅ Tried to launch **{appName}**!";
            }
            catch (Exception ex)
            {
                return $"❌ Couldn't launch **{appName}**: {ex.Message}\n\n" +
                    "Try: `find app [name]` to search for it, or `learn app [name]` to teach me where it is.";
            }
        }

        /// <summary>
        /// Focus on a running application window.
        /// </summary>
        public bool FocusWindow(string appName)
        {
            var processes = Process.GetProcesses();
            foreach (var proc in processes)
            {
                try
                {
                    if (proc.MainWindowTitle.ToLowerInvariant().Contains(appName.ToLowerInvariant()) ||
                        proc.ProcessName.ToLowerInvariant().Contains(appName.ToLowerInvariant()))
                    {
                        IntPtr handle = proc.MainWindowHandle;
                        if (handle != IntPtr.Zero)
                        {
                            if (IsIconic(handle))
                                ShowWindow(handle, 9); // Restore
                            SetForegroundWindow(handle);
                            return true;
                        }
                    }
                }
                catch { }
            }
            return false;
        }

        /// <summary>
        /// List all running applications.
        /// </summary>
        public List<string> GetRunningApps()
        {
            var apps = new List<string>();
            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    if (!string.IsNullOrEmpty(proc.MainWindowTitle) && proc.MainWindowHandle != IntPtr.Zero)
                    {
                        apps.Add($"{proc.ProcessName} — {proc.MainWindowTitle}");
                    }
                }
                catch { }
            }
            return apps.OrderBy(a => a).ToList();
        }

        /// <summary>
        /// Close an application.
        /// </summary>
        public string CloseApp(string appName)
        {
            var processes = Process.GetProcesses();
            foreach (var proc in processes)
            {
                try
                {
                    if (proc.ProcessName.ToLowerInvariant().Contains(appName.ToLowerInvariant()))
                    {
                        proc.CloseMainWindow();
                        proc.WaitForExit(3000);
                        if (!proc.HasExited)
                            proc.Kill();
                        return $"✅ Closed **{appName}**!";
                    }
                }
                catch { }
            }
            return $"❌ **{appName}** doesn't seem to be running.";
        }

        #endregion

        #region Software Learning

        /// <summary>
        /// Learn about a new software application.
        /// </summary>
        public string LearnSoftware(string name, string executablePath, string description = "")
        {
            var existing = _software.FirstOrDefault(s =>
                s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.ExecutablePath = executablePath;
                existing.Description = description;
                existing.LastUpdated = DateTime.Now;
            }
            else
            {
                _software.Add(new SoftwareInfo
                {
                    Name = name,
                    ExecutablePath = executablePath,
                    Description = description,
                    LearnedAt = DateTime.Now,
                    LastUpdated = DateTime.Now
                });
            }

            SaveSoftware();
            return $"📚 Learned about **{name}**! Path: `{executablePath}`";
        }

        /// <summary>
        /// Learn a procedure (how to do something in an app).
        /// Rama asks the user how to do it, then remembers forever.
        /// </summary>
        public string LearnProcedure(string appName, string task, List<string> steps)
        {
            var procedure = new LearnedProcedure
            {
                AppName = appName,
                Task = task,
                Steps = steps,
                LearnedAt = DateTime.Now,
                UseCount = 0
            };

            // Check if exists
            var existing = _procedures.FirstOrDefault(p =>
                p.AppName.Equals(appName, StringComparison.OrdinalIgnoreCase) &&
                p.Task.Equals(task, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.Steps = steps;
                existing.LearnedAt = DateTime.Now;
            }
            else
            {
                _procedures.Add(procedure);
            }

            SaveProcedures();
            return $"🎓 Learned how to **{task}** in **{appName}**!\n\n" +
                   $"Steps:\n{string.Join("\n", steps.Select((s, i) => $"{i + 1}. {s}"))}\n\n" +
                   "I'll remember this forever! Next time just say the task and I'll do it.";
        }

        /// <summary>
        /// Perform a learned procedure.
        /// </summary>
        public async Task<string> PerformProcedure(string appName, string task)
        {
            var procedure = _procedures.FirstOrDefault(p =>
                p.AppName.Equals(appName, StringComparison.OrdinalIgnoreCase) &&
                p.Task.ToLowerInvariant().Contains(task.ToLowerInvariant()));

            if (procedure == null)
            {
                return $"🤔 I don't know how to **{task}** in **{appName}** yet.\n\n" +
                       $"Teach me! Say:\n" +
                       $"`learn [app] [task] step 1: [do this] step 2: [do that]...`\n\n" +
                       $"Example: `learn chrome open new tab step 1: press Ctrl+T`";
            }

            // Execute the procedure
            procedure.UseCount++;
            procedure.LastUsed = DateTime.Now;
            SaveProcedures();

            var sb = new StringBuilder();
            sb.AppendLine($"🔧 Performing: **{task}** in **{appName}**\n");

            // Focus the window first
            FocusWindow(appName);
            await Task.Delay(500);

            for (int i = 0; i < procedure.Steps.Count; i++)
            {
                string step = procedure.Steps[i];
                sb.AppendLine($"Step {i + 1}: {step}");

                // Parse and execute step
                await ExecuteStep(step);
                await Task.Delay(300);
            }

            sb.AppendLine($"\n✅ Done! Task completed.");
            return sb.ToString();
        }

        /// <summary>
        /// Ask the user how to do something and learn from their answer.
        /// </summary>
        public string AskHowToDo(string appName, string task)
        {
            return $"🤔 I don't know how to **{task}** in **{appName}** yet.\n\n" +
                   "Can you teach me? Just tell me the steps!\n\n" +
                   "Example:\n" +
                   $"`learn {appName} {task} step 1: open the app step 2: click File menu step 3: click Save`\n\n" +
                   "Or just describe what you do, and I'll figure out the steps!";
        }

        /// <summary>
        /// Get all learned procedures for an app.
        /// </summary>
        public List<LearnedProcedure> GetProcedures(string appName)
        {
            return _procedures
                .Where(p => p.AppName.Equals(appName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.UseCount)
                .ToList();
        }

        /// <summary>
        /// List all known software.
        /// </summary>
        public List<SoftwareInfo> GetKnownSoftware()
        {
            return _software.OrderBy(s => s.Name).ToList();
        }

        #endregion

        #region Auto-Scan

        private void ScanInstalledSoftware()
        {
            // Scan common installation paths
            string[] scanPaths = {
                @"C:\Program Files",
                @"C:\Program Files (x86)",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs))
            };

            foreach (var basePath in scanPaths)
            {
                if (!Directory.Exists(basePath)) continue;

                try
                {
                    foreach (var dir in Directory.GetDirectories(basePath).Take(100))
                    {
                        var exes = Directory.GetFiles(dir, "*.exe", SearchOption.TopDirectoryOnly);
                        foreach (var exe in exes.Take(3))
                        {
                            string name = Path.GetFileNameWithoutExtension(exe);
                            if (!_software.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                            {
                                _software.Add(new SoftwareInfo
                                {
                                    Name = name,
                                    ExecutablePath = exe,
                                    Description = "Auto-detected",
                                    LearnedAt = DateTime.Now,
                                    AutoDetected = true
                                });
                            }
                        }
                    }
                }
                catch { }
            }

            // Add common apps
            AddCommonApp("Chrome", @"C:\Program Files\Google\Chrome\Application\chrome.exe");
            AddCommonApp("Firefox", @"C:\Program Files\Mozilla Firefox\firefox.exe");
            AddCommonApp("Edge", @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe");
            AddCommonApp("VS Code", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Microsoft VS Code\Code.exe"));
            AddCommonApp("Notepad", "notepad.exe");
            AddCommonApp("Calculator", "calc.exe");
            AddCommonApp("Paint", "mspaint.exe");
            AddCommonApp("Word", "winword.exe");
            AddCommonApp("Excel", "excel.exe");
            AddCommonApp("PowerPoint", "powerpnt.exe");
            AddCommonApp("Outlook", "outlook.exe");
            AddCommonApp("Spotify", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Spotify\Spotify.exe"));
            AddCommonApp("Discord", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Discord\app-*\Discord.exe"));
            AddCommonApp("Teams", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Teams\current\Teams.exe"));
            AddCommonApp("Zoom", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Zoom\bin\Zoom.exe"));
            AddCommonApp("Photoshop", @"C:\Program Files\Adobe\Adobe Photoshop *\Photoshop.exe");
            AddCommonApp("File Explorer", "explorer.exe");
            AddCommonApp("Task Manager", "taskmgr.exe");
            AddCommonApp("Control Panel", "control.exe");

            SaveSoftware();
        }

        private void AddCommonApp(string name, string path)
        {
            if (!_software.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                _software.Add(new SoftwareInfo
                {
                    Name = name,
                    ExecutablePath = path,
                    Description = "Common application",
                    LearnedAt = DateTime.Now,
                    AutoDetected = true
                });
            }
        }

        private SoftwareInfo? FindSoftware(string name)
        {
            return _software.FirstOrDefault(s =>
                s.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                s.Name.ToLowerInvariant().Contains(name.ToLowerInvariant()));
        }

        private string? FindExecutable(string name)
        {
            // Try PATH
            try
            {
                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = name,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                proc?.WaitForExit(2000);
                string output = proc?.StandardOutput.ReadToEnd()?.Trim() ?? "";
                if (!string.IsNullOrEmpty(output) && File.Exists(output.Split('\n')[0].Trim()))
                    return output.Split('\n')[0].Trim();
            }
            catch { }

            // Try common extensions
            string[] extensions = { ".exe", ".bat", ".cmd", ".msc" };
            foreach (var ext in extensions)
            {
                try
                {
                    var result = Process.Start(new ProcessStartInfo
                    {
                        FileName = "where",
                        Arguments = name + ext,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                    result?.WaitForExit(2000);
                    string output = result?.StandardOutput.ReadToEnd()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(output) && File.Exists(output.Split('\n')[0].Trim()))
                        return output.Split('\n')[0].Trim();
                }
                catch { }
            }

            return null;
        }

        #endregion

        #region Step Execution

        private async Task ExecuteStep(string step)
        {
            string lower = step.ToLowerInvariant();

            // Keyboard shortcuts
            if (lower.Contains("press "))
            {
                string key = ExtractAfter(step, "press ");
                SendKeys(key);
            }
            // Type text
            else if (lower.Contains("type "))
            {
                string text = ExtractAfter(step, "type ");
                SendKeys.SendWait(text);
            }
            // Click
            else if (lower.Contains("click "))
            {
                string target = ExtractAfter(step, "click ");
                // For now, use SendKeys for menu items
                if (target.ToLowerInvariant().Contains("file"))
                    SendKeys.SendWait("%f"); // Alt+F
                else if (target.ToLowerInvariant().Contains("edit"))
                    SendKeys.SendWait("%e"); // Alt+E
                else if (target.ToLowerInvariant().Contains("save"))
                    SendKeys.SendWait("^s"); // Ctrl+S
            }
            // Wait
            else if (lower.Contains("wait"))
            {
                string timeStr = System.Text.RegularExpressions.Regex.Match(lower, @"(\d+)").Value;
                int ms = int.TryParse(timeStr, out int t) ? t : 500;
                await Task.Delay(ms);
            }

            await Task.Delay(100);
        }

        private void SendKeys(string keys)
        {
            try
            {
                // Convert common shortcuts
                keys = keys.Replace("Ctrl+", "^")
                          .Replace("Alt+", "%")
                          .Replace("Shift+", "+")
                          .Replace("Enter", "{ENTER}")
                          .Replace("Tab", "{TAB}")
                          .Replace("Escape", "{ESC}")
                          .Replace("Delete", "{DEL}")
                          .Replace("Backspace", "{BACKSPACE}");

                System.Windows.Forms.SendKeys.SendWait(keys);
            }
            catch { }
        }

        private string ExtractAfter(string text, string marker)
        {
            int idx = text.ToLowerInvariant().IndexOf(marker.ToLowerInvariant());
            if (idx < 0) return text;
            return text.Substring(idx + marker.Length).Trim();
        }

        #endregion

        #region Reporting

        public string GetSoftwareReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"📱 **Software Database:** {_software.Count} apps\n");

            // Group by type
            var detected = _software.Where(s => s.AutoDetected).ToList();
            var learned = _software.Where(s => !s.AutoDetected).ToList();

            if (learned.Any())
            {
                sb.AppendLine("📚 **Manually Added:**");
                foreach (var app in learned.Take(10))
                    sb.AppendLine($"  • **{app.Name}** — {app.ExecutablePath}");
                sb.AppendLine();
            }

            sb.AppendLine($"🔍 **Auto-detected:** {detected.Count} apps");
            sb.AppendLine($"📝 **Procedures learned:** {_procedures.Count}");

            if (_procedures.Any())
            {
                sb.AppendLine("\n🎓 **Learned Procedures:**");
                foreach (var proc in _procedures.OrderByDescending(p => p.UseCount).Take(10))
                    sb.AppendLine($"  • **{proc.AppName}**: {proc.Task} (used {proc.UseCount}x)");
            }

            return sb.ToString();
        }

        #endregion

        #region Persistence

        private void LoadAll()
        {
            try
            {
                if (File.Exists(SoftwareDbPath))
                    _software = JsonConvert.DeserializeObject<List<SoftwareInfo>>(File.ReadAllText(SoftwareDbPath)) ?? new();
                if (File.Exists(ProceduresPath))
                    _procedures = JsonConvert.DeserializeObject<List<LearnedProcedure>>(File.ReadAllText(ProceduresPath)) ?? new();
            }
            catch { }
        }

        private void SaveSoftware() =>
            File.WriteAllText(SoftwareDbPath, JsonConvert.SerializeObject(_software, Formatting.Indented));

        private void SaveProcedures() =>
            File.WriteAllText(ProceduresPath, JsonConvert.SerializeObject(_procedures, Formatting.Indented));

        #endregion

        public void Dispose()
        {
            SaveSoftware();
            SaveProcedures();
        }
    }

    #region Models

    public class SoftwareInfo
    {
        public string Name { get; set; } = "";
        public string ExecutablePath { get; set; } = "";
        public string Description { get; set; } = "";
        public bool AutoDetected { get; set; }
        public DateTime LearnedAt { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class LearnedProcedure
    {
        public string AppName { get; set; } = "";
        public string Task { get; set; } = "";
        public List<string> Steps { get; set; } = new();
        public int UseCount { get; set; }
        public DateTime LearnedAt { get; set; }
        public DateTime LastUsed { get; set; }
    }

    #endregion
}
