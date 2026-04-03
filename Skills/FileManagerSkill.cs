using System.Diagnostics;

namespace Rama.Skills
{
    /// <summary>
    /// Performs file and directory management operations.
    /// Supports: listing files, creating/deleting/renaming files and folders,
    /// opening folders in Explorer, copying/moving files, and searching for files.
    /// </summary>
    public class FileManagerSkill : SkillBase
    {
        public override string Name => "File Manager";

        public override string Description => "Manage files and folders — list, create, delete, rename, copy, move";

        public override string[] Triggers => new[]
        {
            "list files", "show files", "list directory", "show folder",
            "create file", "create folder", "make folder", "make directory",
            "delete file", "delete folder", "remove file",
            "rename file", "rename folder",
            "copy file", "move file",
            "open folder", "open directory", "browse folder",
            "find file", "search file",
            "file manager", "manage files"
        };

        public override bool CanHandle(string input)
        {
            var lower = input.ToLowerInvariant();
            return Triggers.Any(t => lower.Contains(t)) ||
                   lower.Contains("file") && (lower.Contains("create") || lower.Contains("delete") ||
                   lower.Contains("list") || lower.Contains("show") || lower.Contains("open"));
        }

        public override Task<string> ExecuteAsync(string input, Core.Memory memory)
        {
            var lower = input.ToLowerInvariant();

            try
            {
                // List files in a directory
                if (lower.Contains("list files") || lower.Contains("show files") ||
                    lower.Contains("list directory") || lower.Contains("show folder"))
                {
                    return Task.FromResult(ListFiles(input));
                }

                // Create folder
                if (lower.Contains("create folder") || lower.Contains("make folder") ||
                    lower.Contains("make directory") || lower.Contains("create directory"))
                {
                    return Task.FromResult(CreateFolder(input));
                }

                // Delete file/folder
                if (lower.Contains("delete file") || lower.Contains("delete folder") ||
                    lower.Contains("remove file") || lower.Contains("remove folder"))
                {
                    return Task.FromResult(DeleteFileOrFolder(input));
                }

                // Rename
                if (lower.Contains("rename"))
                {
                    return Task.FromResult(RenameItem(input));
                }

                // Open folder in Explorer
                if (lower.Contains("open folder") || lower.Contains("open directory") ||
                    lower.Contains("browse folder"))
                {
                    return Task.FromResult(OpenFolder(input));
                }

                // Find/search file
                if (lower.Contains("find file") || lower.Contains("search file") ||
                    lower.Contains("search for file"))
                {
                    return Task.FromResult(FindFile(input));
                }

                return Task.FromResult(
                    "I can help with file management! Try:\n" +
                    "• \"list files in C:\\Users\"\n" +
                    "• \"create folder NewProject on Desktop\"\n" +
                    "• \"delete file temp.txt\"\n" +
                    "• \"open folder Documents\"\n" +
                    "• \"find file readme.md\"");
            }
            catch (Exception ex)
            {
                return Task.FromResult($"File operation failed: {ex.Message}");
            }
        }

        private string ListFiles(string input)
        {
            var path = ExtractPath(input);
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            if (!Directory.Exists(path))
                return $"Directory not found: **{path}**";

            var dirs = Directory.GetDirectories(path).Take(20);
            var files = Directory.GetFiles(path).Take(30);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"📁 Contents of **{path}**:");

            var dirList = dirs.ToList();
            if (dirList.Count > 0)
            {
                sb.AppendLine("\n**Folders:**");
                foreach (var dir in dirList)
                {
                    var name = Path.GetFileName(dir);
                    var lastMod = Directory.GetLastWriteTime(dir).ToString("yyyy-MM-dd HH:mm");
                    sb.AppendLine($"  📁 {name}  ({lastMod})");
                }
            }

            var fileList = files.ToList();
            if (fileList.Count > 0)
            {
                sb.AppendLine("\n**Files:**");
                foreach (var file in fileList)
                {
                    var name = Path.GetFileName(file);
                    var size = new FileInfo(file).Length;
                    var sizeStr = FormatFileSize(size);
                    var lastMod = File.GetLastWriteTime(file).ToString("yyyy-MM-dd HH:mm");
                    sb.AppendLine($"  📄 {name}  ({sizeStr}, {lastMod})");
                }
            }

            if (dirList.Count == 0 && fileList.Count == 0)
                sb.AppendLine("\n_(empty directory)_");

            return sb.ToString();
        }

        private string CreateFolder(string input)
        {
            var path = ExtractPath(input);
            if (string.IsNullOrWhiteSpace(path))
                return "Please specify a path. Example: \"create folder MyProject on Desktop\"";

            // If path is relative, put it on Desktop
            if (!Path.IsPathRooted(path))
            {
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                path = Path.Combine(desktop, path);
            }

            if (Directory.Exists(path))
                return $"Folder already exists: **{path}**";

            Directory.CreateDirectory(path);
            return $"Created folder: **{path}** ✅";
        }

        private string DeleteFileOrFolder(string input)
        {
            var path = ExtractPath(input);
            if (string.IsNullOrWhiteSpace(path))
                return "Please specify what to delete. Example: \"delete file temp.txt\"";

            // Safety: don't delete system directories
            var dangerousPaths = new[] { "C:\\Windows", "C:\\Program Files", "C:\\Users" };
            if (dangerousPaths.Any(dp => path.StartsWith(dp, StringComparison.OrdinalIgnoreCase)))
                return "⛔ I won't delete system directories for safety reasons.";

            if (File.Exists(path))
            {
                File.Delete(path);
                return $"Deleted file: **{path}** 🗑️";
            }
            else if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: false);
                return $"Deleted folder: **{path}** 🗑️";
            }

            return $"Not found: **{path}**";
        }

        private string RenameItem(string input)
        {
            return "To rename files, please specify the current path and new name.\n" +
                   "Example: \"rename C:\\Users\\Desktop\\old.txt to new.txt\"";
        }

        private string OpenFolder(string input)
        {
            var path = ExtractPath(input);
            if (string.IsNullOrWhiteSpace(path))
                path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Handle special folder names
            var specialFolders = new Dictionary<string, Environment.SpecialFolder>(StringComparer.OrdinalIgnoreCase)
            {
                { "desktop", Environment.SpecialFolder.Desktop },
                { "documents", Environment.SpecialFolder.MyDocuments },
                { "downloads", Environment.SpecialFolder.UserProfile },
                { "pictures", Environment.SpecialFolder.MyPictures },
                { "music", Environment.SpecialFolder.MyMusic },
                { "videos", Environment.SpecialFolder.MyVideos },
            };

            if (specialFolders.TryGetValue(path, out var folder))
            {
                path = Environment.GetFolderPath(folder);
                if (path == "downloads" || path == Environment.SpecialFolder.UserProfile.ToString())
                {
                    path = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                }
            }

            if (!Directory.Exists(path))
                return $"Folder not found: **{path}**";

            Process.Start(new ProcessStartInfo("explorer.exe", path) { UseShellExecute = true });
            return $"Opening folder: **{path}** 📂";
        }

        private string FindFile(string input)
        {
            var searchPattern = ExtractAfterTrigger(input)
                .Replace("find file", "", StringComparison.OrdinalIgnoreCase)
                .Replace("search file", "", StringComparison.OrdinalIgnoreCase)
                .Replace("search for file", "", StringComparison.OrdinalIgnoreCase)
                .Trim();

            if (string.IsNullOrWhiteSpace(searchPattern))
                return "What file are you looking for? Example: \"find file readme.md\"";

            // Search in common locations
            var searchPaths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };

            var results = new List<string>();
            foreach (var basePath in searchPaths)
            {
                if (!Directory.Exists(basePath)) continue;
                try
                {
                    var found = Directory.GetFiles(basePath, $"*{searchPattern}*",
                        SearchOption.TopDirectoryOnly);
                    results.AddRange(found);
                }
                catch { /* Access denied on some dirs */ }
            }

            if (results.Count == 0)
                return $"No files matching **\"{searchPattern}\"** found in common locations.";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Found {results.Count} file(s) matching **\"{searchPattern}\"**:");
            foreach (var result in results.Take(15))
            {
                sb.AppendLine($"  📄 {result}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Extracts a file/folder path from the user input.
        /// Handles quoted paths, absolute paths, and relative names.
        /// </summary>
        private string ExtractPath(string input)
        {
            // Check for quoted paths
            var quoteIdx = input.IndexOf('"');
            if (quoteIdx >= 0)
            {
                var endQuote = input.IndexOf('"', quoteIdx + 1);
                if (endQuote > quoteIdx)
                    return input.Substring(quoteIdx + 1, endQuote - quoteIdx - 1);
            }

            // Try to find absolute paths (C:\..., D:\...)
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (part.Length >= 3 && part[1] == ':' && part[2] == '\\')
                    return part;
            }

            // Check for "on desktop", "in documents" etc.
            var lower = input.ToLowerInvariant();
            var onIndex = lower.IndexOf(" on ");
            var atIndex = lower.IndexOf(" at ");
            var inIndex = lower.IndexOf(" in ");

            // Get the name (word after the trigger)
            var name = ExtractAfterTrigger(input);

            // Remove prepositions and folder names from the name
            var prepositions = new[] { " on ", " at ", " in ", " to " };
            foreach (var prep in prepositions)
            {
                var idx = name.ToLowerInvariant().IndexOf(prep);
                if (idx > 0)
                    name = name.Substring(0, idx).Trim();
            }

            // Check if a target folder is specified
            string targetFolder = "";
            if (onIndex > 0 || atIndex > 0 || inIndex > 0)
            {
                var folderIdx = Math.Max(onIndex, Math.Max(atIndex, inIndex));
                var folderPart = input.Substring(folderIdx + 4).Trim().ToLowerInvariant();

                var specialFolders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "desktop", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) },
                    { "documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) },
                    { "my documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) },
                    { "downloads", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads") },
                    { "pictures", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) },
                    { "home", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) },
                };

                if (specialFolders.TryGetValue(folderPart, out var resolved))
                    targetFolder = resolved;
            }

            if (!string.IsNullOrEmpty(targetFolder) && !string.IsNullOrEmpty(name))
                return Path.Combine(targetFolder, name);

            if (!string.IsNullOrEmpty(name) && Path.IsPathRooted(name))
                return name;

            if (!string.IsNullOrEmpty(targetFolder))
                return targetFolder;

            return name;
        }

        private string FormatFileSize(long bytes)
        {
            return bytes switch
            {
                < 1024 => $"{bytes} B",
                < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
                < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
                _ => $"{bytes / (1024.0 * 1024 * 1024):F1} GB"
            };
        }
    }
}
