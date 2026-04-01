using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Rama.Skills
{
    /// <summary>
    /// Provides system information and diagnostics.
    /// Displays OS info, CPU/memory usage, network status, uptime, and more.
    /// </summary>
    public class SystemInfoSkill : SkillBase
    {
        public override string Name => "System Info";

        public override string Description => "Get information about your computer — OS, memory, network, uptime";

        public override string[] Triggers => new[]
        {
            "system info", "system information", "computer info", "pc info",
            "cpu", "memory", "ram", "disk space", "uptime",
            "ip address", "hostname", "os version", "whoami",
            "task list", "running processes", "battery"
        };

        public override bool CanHandle(string input)
        {
            var lower = input.ToLowerInvariant();
            return Triggers.Any(t => lower.Contains(t));
        }

        public override Task<string> ExecuteAsync(string input, Core.Memory memory)
        {
            var lower = input.ToLowerInvariant();

            // Specific queries
            if (lower.Contains("cpu") && !lower.Contains("info"))
                return Task.FromResult(GetCpuInfo());
            if (lower.Contains("memory") || lower.Contains("ram"))
                return Task.FromResult(GetMemoryInfo());
            if (lower.Contains("disk") || lower.Contains("drive"))
                return Task.FromResult(GetDiskInfo());
            if (lower.Contains("ip") || lower.Contains("network"))
                return Task.FromResult(GetNetworkInfo());
            if (lower.Contains("uptime"))
                return Task.FromResult(GetUptime());
            if (lower.Contains("process") || lower.Contains("task list"))
                return Task.FromResult(GetProcessInfo());
            if (lower.Contains("whoami"))
                return Task.FromResult(GetUserInfo());
            if (lower.Contains("battery"))
                return Task.FromResult(GetBatteryInfo());

            // General system info
            return Task.FromResult(GetFullSystemInfo());
        }

        private string GetFullSystemInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("🖥️ **System Information**\n");

            // OS Info
            sb.AppendLine($"**Operating System:** {RuntimeInformation.OSDescription}");
            sb.AppendLine($"**Architecture:** {RuntimeInformation.OSArchitecture}");
            sb.AppendLine($"**.NET Version:** {RuntimeInformation.FrameworkDescription}");
            sb.AppendLine($"**Machine Name:** {Environment.MachineName}");
            sb.AppendLine($"**User:** {Environment.UserName}");
            sb.AppendLine();

            // CPU
            sb.AppendLine($"**Processor Count:** {Environment.ProcessorCount} cores");
            sb.AppendLine();

            // Memory
            var totalMemory = GC.GetTotalMemory(false);
            sb.AppendLine($"**Managed Memory:** {FormatBytes(totalMemory)}");
            sb.AppendLine();

            // Drives
            sb.AppendLine("**Drives:**");
            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                var freeGB = drive.AvailableFreeSpace / (1024.0 * 1024 * 1024);
                var totalGB = drive.TotalSize / (1024.0 * 1024 * 1024);
                var usedPercent = ((totalGB - freeGB) / totalGB) * 100;
                sb.AppendLine($"  {drive.Name} ({drive.DriveType}) — " +
                    $"{freeGB:F1} GB free / {totalGB:F1} GB ({usedPercent:F0}% used)");
            }
            sb.AppendLine();

            // Uptime
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            sb.AppendLine($"**Uptime:** {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m");
            sb.AppendLine();

            // Network
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            sb.AppendLine("**Active Network Interfaces:**");
            foreach (var ni in networkInterfaces.Take(5))
            {
                var ip = ni.GetIPProperties().UnicastAddresses
                    .FirstOrDefault(a => a.Address.AddressFamily ==
                        System.Net.Sockets.AddressFamily.InterNetwork);
                sb.AppendLine($"  {ni.Name} ({ni.NetworkInterfaceType}) — " +
                    $"{ip?.Address.ToString() ?? "N/A"}");
            }

            return sb.ToString();
        }

        private string GetCpuInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("⚡ **CPU Information**\n");
            sb.AppendLine($"**Logical Processors:** {Environment.ProcessorCount}");
            sb.AppendLine($"**Architecture:** {RuntimeInformation.OSArchitecture}");

            try
            {
                // Try to get CPU name from environment
                var procInfo = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
                if (!string.IsNullOrEmpty(procInfo))
                    sb.AppendLine($"**CPU:** {procInfo}");
            }
            catch { }

            sb.AppendLine($"**64-bit OS:** {Environment.Is64BitOperatingSystem}");
            sb.AppendLine($"**64-bit Process:** {Environment.Is64BitProcess}");

            return sb.ToString();
        }

        private string GetMemoryInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("💾 **Memory Information**\n");

            var managed = GC.GetTotalMemory(false);
            sb.AppendLine($"**Managed Memory:** {FormatBytes(managed)}");
            sb.AppendLine($"**GC Gen 0 Collections:** {GC.CollectionCount(0)}");
            sb.AppendLine($"**GC Gen 1 Collections:** {GC.CollectionCount(1)}");
            sb.AppendLine($"**GC Gen 2 Collections:** {GC.CollectionCount(2)}");

            // Total committed virtual memory
            var committed = Environment.WorkingSet;
            sb.AppendLine($"**Working Set:** {FormatBytes(committed)}");

            return sb.ToString();
        }

        private string GetDiskInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("💿 **Disk Information**\n");

            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                var freeGB = drive.AvailableFreeSpace / (1024.0 * 1024 * 1024);
                var totalGB = drive.TotalSize / (1024.0 * 1024 * 1024);
                var usedPercent = ((totalGB - freeGB) / totalGB) * 100;
                var bar = GenerateBar(usedPercent);

                sb.AppendLine($"**{drive.Name}** ({drive.DriveType}) — {drive.VolumeLabel}");
                sb.AppendLine($"  {bar} {usedPercent:F0}% used");
                sb.AppendLine($"  {FormatBytes(drive.AvailableFreeSpace)} free of {FormatBytes(drive.TotalSize)}");
                sb.AppendLine($"  Format: {drive.DriveFormat}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string GetNetworkInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("🌐 **Network Information**\n");

            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up);

            foreach (var ni in interfaces)
            {
                var ipProps = ni.GetIPProperties();
                var ipv4 = ipProps.UnicastAddresses
                    .FirstOrDefault(a => a.Address.AddressFamily ==
                        System.Net.Sockets.AddressFamily.InterNetwork);

                sb.AppendLine($"**{ni.Name}** ({ni.NetworkInterfaceType})");
                if (ipv4 != null)
                    sb.AppendLine($"  IP: {ipv4.Address}");
                if (ipProps.GatewayAddresses.Count > 0)
                    sb.AppendLine($"  Gateway: {ipProps.GatewayAddresses[0].Address}");

                // DNS servers
                var dns = ipProps.DnsAddresses.FirstOrDefault();
                if (dns != null)
                    sb.AppendLine($"  DNS: {dns}");

                sb.AppendLine($"  Status: {ni.OperationalStatus}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string GetUptime()
        {
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            return $"⏱️ System Uptime: **{uptime.Days}** days, **{uptime.Hours}** hours, " +
                   $"**{uptime.Minutes}** minutes, **{uptime.Seconds}** seconds";
        }

        private string GetProcessInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("📋 **Top Processes by Memory**\n");

            var processes = Process.GetProcesses()
                .OrderByDescending(p =>
                {
                    try { return p.WorkingSet64; }
                    catch { return 0; }
                })
                .Take(15);

            sb.AppendLine("| Process | PID | Memory |");
            sb.AppendLine("|---------|-----|--------|");

            foreach (var proc in processes)
            {
                try
                {
                    var mem = FormatBytes(proc.WorkingSet64);
                    sb.AppendLine($"| {proc.ProcessName,-20} | {proc.Id,6} | {mem,10} |");
                }
                catch { /* Process may have exited */ }
            }

            return sb.ToString();
        }

        private string GetUserInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("👤 **User Information**\n");
            sb.AppendLine($"**Username:** {Environment.UserName}");
            sb.AppendLine($"**Domain:** {Environment.UserDomainName}");
            sb.AppendLine($"**Machine:** {Environment.MachineName}");
            sb.AppendLine($"**Interactive:** {Environment.UserInteractive}");
            sb.AppendLine($"**Special Folders:**");
            sb.AppendLine($"  Desktop: {Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}");
            sb.AppendLine($"  Documents: {Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}");
            sb.AppendLine($"  Home: {Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}");

            return sb.ToString();
        }

        private string GetBatteryInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("🔋 **Battery Information**\n");

            var status = System.Windows.Forms.SystemInformation.PowerStatus;
            sb.AppendLine($"**Power Status:** {status.PowerLineStatus}");
            sb.AppendLine($"**Battery Charge Status:** {status.BatteryChargeStatus}");
            sb.AppendLine($"**Battery Life Percent:** {status.BatteryLifePercent * 100:F0}%");

            if (status.BatteryLifeRemaining > 0)
            {
                var remaining = TimeSpan.FromSeconds(status.BatteryLifeRemaining);
                sb.AppendLine($"**Battery Life Remaining:** {remaining.Hours}h {remaining.Minutes}m");
            }

            return sb.ToString();
        }

        private string FormatBytes(long bytes)
        {
            return bytes switch
            {
                < 1024 => $"{bytes} B",
                < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
                < 1024L * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
                < 1024L * 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024 * 1024):F2} GB",
                _ => $"{bytes / (1024.0 * 1024 * 1024 * 1024):F2} TB"
            };
        }

        private string GenerateBar(double percent)
        {
            var filled = (int)(percent / 5);
            var empty = 20 - filled;
            return "[" + new string('█', filled) + new string('░', empty) + "]";
        }
    }
}
