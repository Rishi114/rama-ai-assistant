using System;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Handles greetings and basic conversational responses.
    /// </summary>
    public class GreetingSkill : SkillBase
    {
        private static readonly Random _random = new();

        public override string Name => "Greeting";
        public override string Description => "Conversational responses";
        public override string[] Triggers => new[] { "hello", "hi", "hey", "good morning", "good afternoon", "good evening", "bye", "goodbye", "how are you" };

        public override bool CanHandle(string input)
        {
            string lower = input.ToLowerInvariant().Trim();
            return lower == "hello" || lower == "hi" || lower == "hey" ||
                   lower.StartsWith("good morning") || lower.StartsWith("good afternoon") ||
                   lower.StartsWith("good evening") || lower == "bye" || lower == "goodbye" ||
                   lower == "how are you" || lower == "what's up" || lower == "sup";
        }

        public override Task<string> ExecuteAsync(string input, Memory memory)
        {
            string lower = input.ToLowerInvariant().Trim();
            string hour = DateTime.Now.Hour switch
            {
                < 12 => "morning",
                < 17 => "afternoon",
                _ => "evening"
            };

            string response;

            if (lower.Contains("good morning"))
                response = GetRandom(new[] { 
                    $"Good {hour}! ☀️ Ready to help!", 
                    $"Good {hour}! What's on your mind?",
                    $"Rise and shine! What can I do for you?" 
                });
            else if (lower == "bye" || lower == "goodbye")
                response = GetRandom(new[] {
                    "Goodbye! 👋 I'll be here when you need me.",
                    "See you later! Don't be a stranger. 😊",
                    "Bye! Have a great day! 🌟"
                });
            else if (lower == "how are you")
                response = GetRandom(new[] {
                    "I'm doing great, thanks for asking! All circuits firing. ⚡",
                    "Better now that we're chatting! What's up?",
                    "Running at full capacity! How can I help?"
                });
            else if (lower == "what's up" || lower == "sup")
                response = GetRandom(new[] {
                    "Just waiting for your commands! 🤖",
                    "Not much — just learning and getting smarter. You?",
                    "Ready to help with whatever you need!"
                });
            else
                response = GetRandom(new[] {
                    $"Hey! Good {hour}! How can I help? 😊",
                    $"Hi there! What can I do for you this {hour}?",
                    "Hello! I'm Rama. What's on your mind?",
                    "Hey! Ready when you are. 🚀"
                });

            return Task.FromResult(response);
        }

        private string GetRandom(string[] options)
        {
            return options[_random.Next(options.Length)];
        }
    }
}
