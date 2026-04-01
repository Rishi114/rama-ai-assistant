using Newtonsoft.Json;

namespace Rama.Skills
{
    /// <summary>
    /// Manages personal notes stored in a JSON file.
    /// Supports creating, listing, searching, and deleting notes.
    /// Notes persist across sessions in the Data directory.
    /// </summary>
    public class NoteSkill : SkillBase
    {
        public override string Name => "Notes";

        public override string Description => "Take, list, search, and delete personal notes";

        public override string[] Triggers => new[]
        {
            "note", "notes", "take note", "write note", "add note",
            "list notes", "show notes", "my notes", "delete note",
            "search notes", "find note", "remember this", "remind me of"
        };

        private string NotesFilePath => Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Data", "notes.json");

        private List<NoteItem> _notes = new();

        public override void OnLoad()
        {
            LoadNotes();
        }

        public override void OnUnload()
        {
            SaveNotes();
        }

        public override bool CanHandle(string input)
        {
            var lower = input.ToLowerInvariant();
            return Triggers.Any(t => lower.Contains(t));
        }

        public override Task<string> ExecuteAsync(string input, Core.Memory memory)
        {
            var lower = input.ToLowerInvariant();

            // Add a note
            if (lower.StartsWith("take note") || lower.StartsWith("add note") ||
                lower.StartsWith("write note") || lower.StartsWith("note: ") ||
                lower.StartsWith("note ") || lower == "new note")
            {
                return Task.FromResult(AddNote(input));
            }

            // Remember this
            if (lower.StartsWith("remember this") || lower.StartsWith("remember:"))
            {
                var content = input.Substring(lower.IndexOf(':') > 0
                    ? lower.IndexOf(':') + 1
                    : lower.IndexOf("this") + 4).Trim();
                if (string.IsNullOrWhiteSpace(content))
                    return Task.FromResult("What should I remember? Example: \"remember this: my wifi password is abc123\"");
                return Task.FromResult(AddNoteDirect("Remembered", content));
            }

            // List notes
            if (lower.Contains("list notes") || lower.Contains("show notes") ||
                lower.Contains("my notes") || lower == "notes")
            {
                return Task.FromResult(ListNotes());
            }

            // Search notes
            if (lower.Contains("search notes") || lower.Contains("find note") ||
                lower.Contains("search for note"))
            {
                var query = ExtractAfterTrigger(input)
                    .Replace("search notes", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("find note", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("search for note", "", StringComparison.OrdinalIgnoreCase)
                    .Trim();
                return Task.FromResult(SearchNotes(query));
            }

            // Delete note
            if (lower.Contains("delete note") || lower.Contains("remove note"))
            {
                var noteRef = ExtractAfterTrigger(input)
                    .Replace("delete note", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("remove note", "", StringComparison.OrdinalIgnoreCase)
                    .Trim();
                return Task.FromResult(DeleteNote(noteRef));
            }

            return Task.FromResult(ListNotes());
        }

        private string AddNote(string input)
        {
            var content = ExtractAfterTrigger(input);

            // Remove trigger prefixes
            var prefixes = new[] { "take note:", "add note:", "write note:", "note:", "note " };
            foreach (var prefix in prefixes)
            {
                if (content.ToLowerInvariant().StartsWith(prefix.ToLowerInvariant()))
                {
                    content = content.Substring(prefix.Length).Trim();
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(content))
                return "What would you like me to note down? Example: \"note: buy groceries tomorrow\"";

            return AddNoteDirect("Quick Note", content);
        }

        private string AddNoteDirect(string title, string content)
        {
            var note = new NoteItem
            {
                Id = _notes.Count > 0 ? _notes.Max(n => n.Id) + 1 : 1,
                Title = title,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _notes.Add(note);
            SaveNotes();

            return $"📝 Note saved (#{note.Id}): **{title}**\n> {content}";
        }

        private string ListNotes()
        {
            if (_notes.Count == 0)
                return "You don't have any notes yet. Try: \"take note: buy groceries\"";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"📝 You have **{_notes.Count}** note(s):\n");

            foreach (var note in _notes.OrderByDescending(n => n.CreatedAt).Take(20))
            {
                var date = note.CreatedAt.ToLocalTime().ToString("MMM dd, HH:mm");
                var preview = note.Content.Length > 60
                    ? note.Content[..60] + "..."
                    : note.Content;
                sb.AppendLine($"  **#{note.Id}** ({date}) — {preview}");
            }

            if (_notes.Count > 20)
                sb.AppendLine($"\n_...and {_notes.Count - 20} more. Use \"search notes\" to find specific ones._");

            return sb.ToString();
        }

        private string SearchNotes(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return "What should I search for? Example: \"search notes groceries\"";

            var results = _notes
                .Where(n => n.Content.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           n.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            if (results.Count == 0)
                return $"No notes matching **\"{query}\"** found.";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Found **{results.Count}** note(s) matching *\"{query}\"*:\n");

            foreach (var note in results.Take(10))
            {
                var date = note.CreatedAt.ToLocalTime().ToString("MMM dd, HH:mm");
                sb.AppendLine($"  **#{note.Id}** ({date}) — {note.Content}");
            }

            return sb.ToString();
        }

        private string DeleteNote(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
                return "Which note? Use the note number. Example: \"delete note 3\"";

            if (int.TryParse(reference.Replace("#", ""), out var id))
            {
                var note = _notes.FirstOrDefault(n => n.Id == id);
                if (note == null)
                    return $"Note #{id} not found.";

                _notes.Remove(note);
                SaveNotes();
                return $"Deleted note #{id} 🗑️";
            }

            // Search by content
            var match = _notes.FirstOrDefault(n =>
                n.Content.Contains(reference, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                _notes.Remove(match);
                SaveNotes();
                return $"Deleted note #{match.Id}: \"{match.Content[..Math.Min(50, match.Content.Length)]}...\" 🗑️";
            }

            return $"Couldn't find a note matching \"{reference}\".";
        }

        private void LoadNotes()
        {
            try
            {
                if (File.Exists(NotesFilePath))
                {
                    var json = File.ReadAllText(NotesFilePath);
                    _notes = JsonConvert.DeserializeObject<List<NoteItem>>(json) ?? new();
                }
            }
            catch
            {
                _notes = new List<NoteItem>();
            }
        }

        private void SaveNotes()
        {
            try
            {
                var dir = Path.GetDirectoryName(NotesFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonConvert.SerializeObject(_notes, Formatting.Indented);
                File.WriteAllText(NotesFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save notes: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Represents a single note item stored by the NoteSkill.
    /// </summary>
    public class NoteItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
