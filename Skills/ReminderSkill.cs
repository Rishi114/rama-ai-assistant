using System.Timers;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace Rama.Skills
{
    /// <summary>
    /// Manages timed reminders that notify the user after a specified duration.
    /// Supports natural language time parsing (e.g., "remind me in 10 minutes").
    /// Reminders are stored in a JSON file for persistence across sessions.
    /// </summary>
    public class ReminderSkill : SkillBase
    {
        public override string Name => "Reminders";

        public override string Description => "Set reminders with natural language time expressions";

        public override string[] Triggers => new[]
        {
            "remind me", "set reminder", "reminder", "alarm",
            "remind", "timer", "set a timer", "in how long"
        };

        private string RemindersFilePath => Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Data", "reminders.json");

        private readonly List<ReminderItem> _activeReminders = new();
        private readonly Dictionary<int, Timer> _timers = new();

        /// <summary>
        /// Fired when a reminder fires. The UI subscribes to this to show notifications.
        /// </summary>
        public static event Action<string>? ReminderFired;

        public override void OnLoad()
        {
            LoadReminders();
        }

        public override void OnUnload()
        {
            foreach (var timer in _timers.Values)
                timer.Dispose();
            _timers.Clear();
            SaveReminders();
        }

        public override bool CanHandle(string input)
        {
            var lower = input.ToLowerInvariant();
            return Triggers.Any(t => lower.Contains(t));
        }

        public override Task<string> ExecuteAsync(string input, Core.Memory memory)
        {
            var lower = input.ToLowerInvariant();

            // List reminders
            if (lower.Contains("list reminder") || lower.Contains("show reminder") ||
                lower.Contains("my reminder") || lower.Contains("active reminder") ||
                lower == "reminders" || lower == "remind me")
            {
                return Task.FromResult(ListReminders());
            }

            // Cancel reminder
            if (lower.Contains("cancel reminder") || lower.Contains("delete reminder") ||
                lower.Contains("remove reminder") || lower.Contains("stop reminder"))
            {
                return Task.FromResult(CancelReminder(input));
            }

            // Set a reminder
            return Task.FromResult(SetReminder(input));
        }

        private string SetReminder(string input)
        {
            // Parse "remind me in X minutes/hours/seconds to Y"
            var (success, message, timeText, action) = ParseReminderInput(input);

            if (!success)
                return message;

            // Parse the time duration
            var duration = ParseDuration(timeText);
            if (duration == null)
                return $"I couldn't understand the time \"{timeText}\". Try: " +
                       "\"remind me in 10 minutes to take a break\"";

            var triggerTime = DateTime.UtcNow.Add(duration.Value);
            var reminder = new ReminderItem
            {
                Id = _activeReminders.Count > 0 ? _activeReminders.Max(r => r.Id) + 1 : 1,
                Message = action ?? "Time's up!",
                TriggerTime = triggerTime,
                CreatedAt = DateTime.UtcNow
            };

            _activeReminders.Add(reminder);

            // Set up the timer
            var timer = new Timer(duration.Value.TotalMilliseconds);
            timer.AutoReset = false;
            timer.Elapsed += (s, e) => FireReminder(reminder.Id);
            timer.Start();
            _timers[reminder.Id] = timer;

            SaveReminders();

            var timeDisplay = FormatDuration(duration.Value);
            return $"⏰ Reminder set! I'll remind you in **{timeDisplay}** to:\n> {reminder.Message}";
        }

        private string ListReminders()
        {
            var active = _activeReminders.Where(r => r.TriggerTime > DateTime.UtcNow).ToList();

            if (active.Count == 0)
                return "No active reminders. Set one with: \"remind me in 10 minutes to stretch\"";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"⏰ You have **{active.Count}** active reminder(s):\n");

            foreach (var r in active.OrderBy(r => r.TriggerTime))
            {
                var remaining = r.TriggerTime - DateTime.UtcNow;
                var timeStr = remaining.TotalMinutes < 1
                    ? "in less than a minute"
                    : $"in {FormatDuration(remaining)}";
                sb.AppendLine($"  **#{r.Id}** {timeStr}: {r.Message}");
            }

            return sb.ToString();
        }

        private string CancelReminder(string input)
        {
            var numbers = System.Text.RegularExpressions.Regex.Matches(input, @"\d+");
            if (numbers.Count > 0 && int.TryParse(numbers[0].Value, out var id))
            {
                var reminder = _activeReminders.FirstOrDefault(r => r.Id == id);
                if (reminder == null)
                    return $"Reminder #{id} not found.";

                if (_timers.TryGetValue(id, out var timer))
                {
                    timer.Stop();
                    timer.Dispose();
                    _timers.Remove(id);
                }

                _activeReminders.Remove(reminder);
                SaveReminders();
                return $"Cancelled reminder #{id}: \"{reminder.Message}\" ✅";
            }

            return "Which reminder? Use the number. Example: \"cancel reminder 1\"";
        }

        private void FireReminder(int reminderId)
        {
            var reminder = _activeReminders.FirstOrDefault(r => r.Id == reminderId);
            if (reminder == null) return;

            _activeReminders.Remove(reminder);
            _timers.Remove(reminderId);
            SaveReminders();

            ReminderFired?.Invoke(reminder.Message);
        }

        private (bool success, string message, string timeText, string action) ParseReminderInput(string input)
        {
            var lower = input.ToLowerInvariant();

            // Pattern: "remind me in X to Y"
            var inIndex = lower.IndexOf(" in ");
            if (inIndex < 0)
            {
                // Try "remind me to Y in X"
                var toIndex = lower.IndexOf(" to ");
                if (toIndex > 0)
                {
                    var afterTo = input.Substring(toIndex + 4).Trim();
                    var lastIn = afterTo.LastIndexOf(" in ");
                    if (lastIn > 0)
                    {
                        var action = afterTo.Substring(0, lastIn).Trim();
                        var time = afterTo.Substring(lastIn + 4).Trim();
                        return (true, "", time, action);
                    }
                }

                return (false,
                    "Please specify a time. Example: \"remind me in 10 minutes to take a break\"",
                    "", "");
            }

            var afterIn = input.Substring(inIndex + 4).Trim();

            // Find "to" separator
            var toIdx = afterIn.ToLowerInvariant().IndexOf(" to ");
            if (toIdx < 0)
                toIdx = afterIn.ToLowerInvariant().IndexOf(" that ");

            if (toIdx > 0)
            {
                var timeText = afterIn.Substring(0, toIdx).Trim();
                var action = afterIn.Substring(toIdx + 4).Trim();
                return (true, "", timeText, action);
            }

            return (true, "", afterIn, "Time's up!");
        }

        private TimeSpan? ParseDuration(string timeText)
        {
            var lower = timeText.ToLowerInvariant().Trim();
            var match = System.Text.RegularExpressions.Regex.Match(lower,
                @"(\d+)\s*(second|minute|hour|min|sec|hr|s|m|h)s?");

            if (!match.Success) return null;

            if (!int.TryParse(match.Groups[1].Value, out var value))
                return null;

            var unit = match.Groups[2].Value;
            return unit switch
            {
                "second" or "sec" or "s" => TimeSpan.FromSeconds(value),
                "minute" or "min" or "m" => TimeSpan.FromMinutes(value),
                "hour" or "hr" or "h" => TimeSpan.FromHours(value),
                _ => null
            };
        }

        private string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
            {
                var hours = (int)duration.TotalHours;
                var mins = duration.Minutes;
                return mins > 0 ? $"{hours}h {mins}m" : $"{hours} hour(s)";
            }
            if (duration.TotalMinutes >= 1)
            {
                return $"{(int)duration.TotalMinutes} minute(s)";
            }
            return $"{(int)duration.TotalSeconds} second(s)";
        }

        private void LoadReminders()
        {
            try
            {
                if (File.Exists(RemindersFilePath))
                {
                    var json = File.ReadAllText(RemindersFilePath);
                    _activeReminders.Clear();
                    _activeReminders.AddRange(
                        JsonConvert.DeserializeObject<List<ReminderItem>>(json) ?? new());

                    // Re-create timers for reminders that haven't fired yet
                    foreach (var reminder in _activeReminders.ToList())
                    {
                        var remaining = reminder.TriggerTime - DateTime.UtcNow;
                        if (remaining.TotalMilliseconds <= 0)
                        {
                            // Missed reminder — fire immediately
                            FireReminder(reminder.Id);
                        }
                        else
                        {
                            var timer = new Timer(remaining.TotalMilliseconds);
                            timer.AutoReset = false;
                            timer.Elapsed += (s, e) => FireReminder(reminder.Id);
                            timer.Start();
                            _timers[reminder.Id] = timer;
                        }
                    }
                }
            }
            catch
            {
                _activeReminders.Clear();
            }
        }

        private void SaveReminders()
        {
            try
            {
                var dir = Path.GetDirectoryName(RemindersFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonConvert.SerializeObject(_activeReminders, Formatting.Indented);
                File.WriteAllText(RemindersFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save reminders: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Represents a single reminder item.
    /// </summary>
    public class ReminderItem
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime TriggerTime { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
