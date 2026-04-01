using Rama.Core;
using Rama.Skills;

namespace RamaJokeSkill
{
    /// <summary>
    /// Example external skill: tells jokes and manages a favorite jokes list.
    /// 
    /// This demonstrates how to build a skill that:
    /// - Uses multiple trigger words for different intents
    /// - Overrides CanHandle() for specific matching
    /// - Uses Memory to store user preferences (favorite jokes)
    /// - Manages internal state across sessions via OnLoad/OnUnload
    /// - Handles error cases gracefully
    /// 
    /// To use this as a template for your own skill:
    /// 1. Copy this project
    /// 2. Rename the class and namespace
    /// 3. Implement your own Name, Description, Triggers, and ExecuteAsync
    /// 4. Build and drop the DLL in the Plugins folder
    /// </summary>
    public class JokeSkill : SkillBase
    {
        public override string Name => "Jokes";

        public override string Description => "Tells jokes, manages favorites, and lightens the mood";

        public override string[] Triggers => new[]
        {
            "joke", "funny", "laugh", "humor", "make me laugh",
            "tell me a joke", "knock knock", "favorite jokes"
        };

        private readonly Random _random = new();

        /// <summary>
        /// Built-in joke collection. In a real skill, this might be loaded from a file or API.
        /// </summary>
        private static readonly List<Joke> Jokes = new()
        {
            new Joke("Why do programmers prefer dark mode?",
                     "Because light attracts bugs! 🐛"),
            new Joke("Why was the JavaScript developer sad?",
                     "Because he didn't Node how to Express himself."),
            new Joke("What's a programmer's favorite hangout place?",
                     "Foo Bar! 🍺"),
            new Joke("Why do Java developers wear glasses?",
                     "Because they can't C#! 👓"),
            new Joke("How many programmers does it take to change a light bulb?",
                     "None. That's a hardware problem. 💡"),
            new Joke("What did the router say to the doctor?",
                     "It hurts when IP! 🌐"),
            new Joke("Why did the developer go broke?",
                     "Because he used up all his cache! 💸"),
            new Joke("What's a computer's least favorite food?",
                     "Spam! 🥫"),
            new Joke("Why was the computer cold?",
                     "It left its Windows open! 🪟"),
            new Joke("What do you call 8 hobbits?",
                     "A hobbyte! 🧙"),
            new Joke("How do trees access the internet?",
                     "They log in! 🌳"),
            new Joke("What's the object-oriented way to become wealthy?",
                     "Inheritance! 💰"),
            new Joke("Why do Python programmers have low self-esteem?",
                     "They're constantly comparing themselves to others."),
            new Joke("What did the ocean say to the beach?",
                     "Nothing, it just waved! 🌊"),
            new Joke("What did the coffee report to the police?",
                     "A mugging! ☕"),
            new Joke("Why don't scientists trust atoms?",
                     "Because they make up everything! ⚛️"),
            new Joke("What do you call a fake noodle?",
                     "An impasta! 🍝"),
            new Joke("Why did the scarecrow win an award?",
                     "Because he was outstanding in his field! 🌾"),
            new Joke("I told my wife she was drawing her eyebrows too high.",
                     "She looked surprised. 😮"),
            new Joke("What's the best thing about Switzerland?",
                     "I don't know, but the flag is a big plus! 🇨🇭"),
        };

        /// <summary>
        /// Knock-knock joke sequences (stateful).
        /// </summary>
        private static readonly List<string[]> KnockKnockJokes = new()
        {
            new[] { "Who's there?", "Nobel.", "Nobel who?", "Nobel, so I knocked! 🚪" },
            new[] { "Who's there?", "Tank.", "Tank who?", "You're welcome! 🪖" },
            new[] { "Who's there?", "Cows go.", "Cows go who?", "No, cows go MOO! 🐄" },
            new[] { "Who's there?", "Boo.", "Boo who?", "Don't cry, it's just a joke! 😢" },
            new[] { "Who's there?", "Lettuce.", "Lettuce who?", "Lettuce in, it's cold out here! 🥬" },
        };

        public override bool CanHandle(string input)
        {
            var lower = input.ToLowerInvariant();

            // Specific intent matching
            if (lower.StartsWith("knock knock"))
                return true;
            if (lower.Contains("tell") && lower.Contains("joke"))
                return true;
            if (lower.Contains("favorite joke") || lower.Contains("favourite joke"))
                return true;
            if (lower.Contains("save joke") || lower.Contains("remember joke"))
                return true;

            // General trigger matching via base class
            return base.CanHandle(input);
        }

        public override Task<string> ExecuteAsync(string input, Core.Memory memory)
        {
            var lower = input.ToLowerInvariant();

            // Knock-knock joke
            if (lower.StartsWith("knock knock"))
            {
                return Task.FromResult(TellKnockKnockJoke());
            }

            // Save a favorite joke reference
            if (lower.Contains("save joke") || lower.Contains("remember joke"))
            {
                return Task.FromResult(SaveFavoriteJoke(memory));
            }

            // Show favorite jokes
            if (lower.Contains("favorite") || lower.Contains("favourite"))
            {
                return Task.FromResult(ShowFavoriteJokes(memory));
            }

            // Tell a random joke
            return Task.FromResult(TellRandomJoke(memory));
        }

        /// <summary>
        /// Tells a random joke and records it in recent history.
        /// </summary>
        private string TellRandomJoke(Core.Memory memory)
        {
            var joke = Jokes[_random.Next(Jokes.Count)];

            // Remember the last joke shown (for "save joke" functionality)
            memory.SetPreference("last_joke_setup", joke.Setup);
            memory.SetPreference("last_joke_punchline", joke.Punchline);

            // Track joke count
            var countStr = memory.GetPreference("jokes_told_count");
            var count = (int.TryParse(countStr, out var c) ? c : 0) + 1;
            memory.SetPreference("jokes_told_count", count.ToString());

            return $"😄 **{joke.Setup}**\n\n||{joke.Punchline}||";
        }

        /// <summary>
        /// Tells a knock-knock joke (returns the full sequence at once for simplicity).
        /// </summary>
        private string TellKnockKnockJoke()
        {
            var joke = KnockKnockJokes[_random.Next(KnockKnockJokes.Length)];
            return $"🚪 **Knock knock!**\n\n" +
                   $"_{joke[0]}_\n" +
                   $"**{joke[1]}**\n\n" +
                   $"_{joke[2]}_\n" +
                   $"**{joke[3]}**";
        }

        /// <summary>
        /// Saves the last shown joke as a favorite.
        /// </summary>
        private string SaveFavoriteJoke(Core.Memory memory)
        {
            var setup = memory.GetPreference("last_joke_setup");
            var punchline = memory.GetPreference("last_joke_punchline");

            if (string.IsNullOrEmpty(setup))
                return "I haven't told you a joke yet! Ask me for a joke first, then say \"save joke\".";

            // Get existing favorites
            var favorites = memory.GetPreference("favorite_jokes") ?? "";
            var favoriteList = string.IsNullOrEmpty(favorites)
                ? new List<string>()
                : favorites.Split("|||").ToList();

            // Check if already saved
            if (favoriteList.Any(f => f.StartsWith(setup)))
                return "That joke's already in your favorites! 😄";

            // Add to favorites
            favoriteList.Add($"{setup}:::{punchline}");
            memory.SetPreference("favorite_jokes", string.Join("|||", favoriteList));

            return $"💾 Saved to favorites! You now have **{favoriteList.Count}** favorite joke(s). " +
                   "Say \"show favorites\" to see them all.";
        }

        /// <summary>
        /// Shows the user's saved favorite jokes.
        /// </summary>
        private string ShowFavoriteJokes(Core.Memory memory)
        {
            var favorites = memory.GetPreference("favorite_jokes") ?? "";

            if (string.IsNullOrEmpty(favorites))
                return "You don't have any favorite jokes yet. " +
                       "Ask me for a joke, then say \"save joke\" to add it to your favorites!";

            var favoriteList = favorites.Split("|||").ToList();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"⭐ **Your Favorite Jokes** ({favoriteList.Count}):\n");

            for (int i = 0; i < favoriteList.Count; i++)
            {
                var parts = favoriteList[i].Split(":::");
                if (parts.Length == 2)
                {
                    sb.AppendLine($"**{i + 1}.** {parts[0]}");
                    sb.AppendLine($"   _{parts[1]}_\n");
                }
            }

            return sb.ToString();
        }

        public override void OnLoad()
        {
            System.Diagnostics.Debug.WriteLine("JokeSkill loaded — ready to make people laugh! 😄");
        }

        public override void OnUnload()
        {
            System.Diagnostics.Debug.WriteLine("JokeSkill unloaded — going silent. 🤐");
        }
    }

    /// <summary>
    /// Simple model for a setup/punchline joke.
    /// </summary>
    public class Joke
    {
        public string Setup { get; }
        public string Punchline { get; }

        public Joke(string setup, string punchline)
        {
            Setup = setup;
            Punchline = punchline;
        }
    }
}
