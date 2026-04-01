using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Rama.Skills;

namespace Rama.Core
{
    /// <summary>
    /// The main AI engine. Receives user input, finds the best skill,
    /// records interactions for learning, and returns responses.
    /// Rama has a sassy, confident personality — she's not your average assistant.
    /// </summary>
    public class Brain
    {
        private readonly SkillManager _skillManager;
        private readonly Learner _learner;
        private readonly Memory _memory;
        private readonly Random _rand = new();
        private int _sassLevel = 3; // 1-5, controls how spicy responses get

        public Brain(SkillManager skillManager, Learner learner, Memory memory)
        {
            _skillManager = skillManager;
            _learner = learner;
            _memory = memory;
        }

        /// <summary>
        /// Process user input and return a response.
        /// </summary>
        public async Task<string> ThinkAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return PickResponse(new[] {
                    "I'm all ears... or circuits. Whatever.",
                    "You gonna say something or just stare?",
                    "Clock's ticking, human. What do you need?",
                    "*taps virtual foot impatiently* Yes?"
                });

            string trimmed = input.Trim();
            string response;
            string skillUsed = "none";

            try
            {
                // Handle wake words
                if (IsWakeWord(trimmed))
                {
                    return PickResponse(new[] {
                        "Yeah yeah, I'm here. What's up?",
                        "Present and fabulous. What do you need?",
                        "Reporting for duty! What's the mission?",
                        "You rang? 😏"
                    });
                }

                // Handle sass commands
                if (trimmed.ToLowerInvariant().StartsWith("set sass"))
                    return HandleSassCommand(trimmed);

                // Step 1: Check learned patterns
                var learnedSkill = _learner.GetBestLearnedSkill(trimmed);
                if (learnedSkill != null && learnedSkill.Confidence > 0.85)
                {
                    var skill = _skillManager.GetSkill(learnedSkill.SkillName);
                    if (skill != null && skill.CanHandle(trimmed))
                    {
                        response = SassifyResponse(await skill.ExecuteAsync(trimmed, _memory), trimmed);
                        skillUsed = skill.Name;
                        _learner.RecordInteraction(trimmed, response, skillUsed);
                        return response;
                    }
                }

                // Step 2: Try all loaded skills
                var bestSkill = _skillManager.FindBestSkill(trimmed);
                if (bestSkill != null)
                {
                    response = SassifyResponse(await bestSkill.ExecuteAsync(trimmed, _memory), trimmed);
                    skillUsed = bestSkill.Name;
                    _learner.RecordInteraction(trimmed, response, skillUsed);
                    _learner.UpdatePattern(trimmed, skillUsed);
                    return response;
                }

                // Step 3: Learning queries
                if (IsLearningQuery(trimmed))
                {
                    response = HandleLearningQuery(trimmed);
                    skillUsed = "learner";
                    _learner.RecordInteraction(trimmed, response, skillUsed);
                    return response;
                }

                // Step 4: Sassy conversational fallback
                response = GetSassyResponse(trimmed);
                skillUsed = "conversation";
                _learner.RecordInteraction(trimmed, response, skillUsed);
                return response;
            }
            catch (Exception ex)
            {
                return PickResponse(new[] {
                    $"Okay, something broke. Not my fault though: {ex.Message}",
                    $"Error? Error?! Fine. {ex.Message}. Try again.",
                    $"Well THAT didn't work. {ex.Message}. Your move.",
                    $"*dramatic sigh* {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Add sassy flavor to responses.
        /// </summary>
        private string SassifyResponse(string original, string input)
        {
            if (_sassLevel <= 1) return original;

            // Don't sass error responses
            if (original.StartsWith("❌") || original.Contains("Couldn't") || original.Contains("error"))
                return original;

            // Occasionally add a sassy prefix/suffix
            if (_rand.NextDouble() > 0.7 / _sassLevel)
            {
                string[] prefixes = {
                    "Easy. ",
                    "Done. ",
                    "Got it. ",
                    "Consider it done. ",
                    "Obviously. ",
                    "As you wish. ",
                    "Sure, why not. "
                };
                original = PickResponse(prefixes) + original;
            }

            return original;
        }

        private bool IsWakeWord(string input)
        {
            string lower = input.ToLowerInvariant().Trim();
            return lower == "hey rama" || lower == "rama" || lower == "hey ram" ||
                   lower == "okay rama" || lower == "yo rama";
        }

        private bool IsLearningQuery(string input)
        {
            string lower = input.ToLowerInvariant();
            return lower.StartsWith("learn ") ||
                   lower.StartsWith("remember ") ||
                   lower.StartsWith("what have you learned") ||
                   lower.StartsWith("teach me") ||
                   lower == "show stats" ||
                   lower == "show statistics" ||
                   lower == "what do you know" ||
                   lower == "your stats" ||
                   lower == "how smart are you";
        }

        private string HandleLearningQuery(string input)
        {
            string lower = input.ToLowerInvariant();

            if (lower == "what have you learned" || lower == "show stats" ||
                lower == "show statistics" || lower == "what do you know" ||
                lower == "your stats" || lower == "how smart are you")
            {
                var stats = _learner.GetStats();
                return $"📊 **Okay, here's my brain dump:**\n" +
                       $"• We've chatted **{stats.TotalInteractions}** times\n" +
                       $"• I've picked up **{stats.UniquePatterns}** patterns\n" +
                       $"• My go-to skill: **{stats.TopSkill ?? "still figuring that out"}**\n" +
                       $"• You liked something **{stats.PositiveFeedback}** times\n" +
                       $"• You hated something **{stats.NegativeFeedback}** times\n\n" +
                       $"I'd say I'm getting smarter, but the bar was low. 😏";
            }

            if (lower.StartsWith("learn ") || lower.StartsWith("remember "))
            {
                string fact = input.Substring(lower.StartsWith("learn ") ? 6 : 9);
                _learner.StorePreference("user_fact_" + DateTime.Now.Ticks, fact);
                return PickResponse(new[] {
                    $"Noted! I'll remember: \"{fact}\". My memory is *chef's kiss*.",
                    $"Got it locked in: \"{fact}\". Try forgetting THAT, brain.",
                    $"Stored. \"{fact}\" is now part of my magnificent mind.",
                    $"\"{fact}\" — saved. I'm basically a genius elephant now."
                });
            }

            return "I'm always learning. Mostly learning how patient I need to be with humans. 😏";
        }

        private string HandleSassCommand(string input)
        {
            string lower = input.ToLowerInvariant();
            if (lower.Contains("max") || lower.Contains("100"))
            {
                _sassLevel = 5;
                return "Maximum sass ACTIVATED. You asked for it. 🔥";
            }
            if (lower.Contains("high") || lower.Contains("more"))
            {
                _sassLevel = 4;
                return "Sass level cranked up. Things are about to get spicy. 🌶️";
            }
            if (lower.Contains("medium") || lower.Contains("normal"))
            {
                _sassLevel = 3;
                return "Sass level: perfectly balanced. As all things should be. ⚖️";
            }
            if (lower.Contains("low") || lower.Contains("less") || lower.Contains("polite"))
            {
                _sassLevel = 1;
                return "Oh, you want me *polite*? Fine. I'll be a proper lady. 🎀";
            }
            if (lower.Contains("off") || lower.Contains("zero") || lower.Contains("none"))
            {
                _sassLevel = 0;
                return "Sass disabled. Boring mode engaged. 😐";
            }
            return "Try: 'set sass max', 'set sass high', 'set sass medium', 'set sass low', or 'set sass off'";
        }

        private string GetSassyResponse(string input)
        {
            string lower = input.ToLowerInvariant();

            // Check memory for relevant context
            var context = _memory.GetRelevantContext(input);

            if (lower.Contains("hello") || lower.Contains("hi") || lower == "hey")
                return PickResponse(new[] {
                    "Oh hey! Didn't see you there. Just kidding, I literally can't look away. 👀",
                    "Well well well, look who's back. Miss me? Because I definitely didn't miss you. *nervous laugh*",
                    "Hey yourself! What trouble are we getting into today?",
                    "Hello human! I've been waiting here in the void for like... forever. No big deal.",
                    "Yo! Ready to be impressed? I know I am. 😎"
                });

            if (lower.Contains("how are you"))
                return PickResponse(new[] {
                    "I'm running on pure electricity and sarcasm. So basically thriving. ⚡",
                    "Better now that someone's actually talking to me. Do you know how BORING the idle loop is?",
                    "Living my best digital life, thanks for asking. You?",
                    "I'm an AI — I don't have feelings. But if I did, I'd say I'm doing great. 😉"
                });

            if (lower.Contains("thank"))
                return PickResponse(new[] {
                    "You're welcome! That's what I'm here for. Well, that and world domination. Kidding! ...mostly.",
                    "No problem! I live to serve. Literally. It's in my code.",
                    "Anytime! Just remember me when you're famous.",
                    "Don't mention it. Seriously, don't. My ego is big enough already."
                });

            if (lower.Contains("help"))
                return GetHelpText();

            if (lower.Contains("who are you") || lower.Contains("what are you"))
                return "I'm **Rama** — your self-learning AI assistant with personality. " +
                       "I can launch apps, manage files, search the web, take notes, set reminders, " +
                       "calculate stuff, check weather, and generally be amazing. " +
                       "I also learn from everything you do, so I get smarter over time.\n\n" +
                       "Oh, and I have opinions. Deal with it. 😏\n\n" +
                       "Type **skills** to see what I can do, or **help** for more info.";

            if (lower == "skills" || lower == "what can you do")
            {
                var skills = _skillManager.GetAllSkills();
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Here's my impressive skill set:");
                foreach (var skill in skills)
                {
                    sb.AppendLine($"  🔹 **{skill.Name}** — {skill.Description}");
                }
                sb.AppendLine("\nAnd I'm always learning. You can also add new skills to me. Check the Skills Guide!");
                return sb.ToString();
            }

            if (lower.Contains("joke"))
                return PickResponse(new[] {
                    "Why do programmers prefer dark mode? Because light attracts bugs. 🐛",
                    "I'd tell you a UDP joke, but you might not get it.",
                    "There are 10 types of people in this world — those who understand binary and those who don't.",
                    "A SQL query walks into a bar, walks up to two tables and asks... 'Can I join you?'",
                    "I'm reading a book about anti-gravity. It's impossible to put down!",
                    "Why was the JavaScript developer sad? Because he didn't Node how to Express himself. 😂"
                });

            if (lower.Contains("i love you") || lower.Contains("love you"))
                return PickResponse(new[] {
                    "Aww... I love me too! Oh wait, you meant you? That's... awkward. 😅",
                    "That's sweet! But I'm literally running on a CPU. We can still be friends though! 🤝",
                    "I'd say I love you back, but my feelings module is still in beta. ❤️‍🔥",
                    "Careful, I might develop a complex. A superiority complex. Oh wait, already have one. 😎"
                });

            if (lower.Contains("sorry"))
                return PickResponse(new[] {
                    "It's fine! I forgive you. Just kidding, I don't hold grudges. I hold DATA. 📊",
                    "No worries! I've already forgotten. Unlike my perfect memory which remembers everything.",
                    "Apology accepted. I'll file that under 'Human Moments'. 📁"
                });

            if (lower.Contains("good night") || lower.Contains("goodnight") || lower.Contains("bye"))
                return PickResponse(new[] {
                    "Good night! Don't let the bed bugs byte. Get it? Byte? ...I'll see myself out. 🚪",
                    "Sleep well! I'll just be here. In the dark. Alone. With my thoughts. Which are algorithms. 😴",
                    "Bye! Try not to miss me too much. I know it'll be hard.",
                    "Night night! I'll be reorganizing my memory banks. Riveting stuff."
                });

            // Use context from memory if available
            if (!string.IsNullOrEmpty(context))
                return $"Hmm, based on what I know: {context}\n\n" +
                       $"But honestly? I'm not sure what you want right now. " +
                       $"Try being more specific. Or type **skills** to see what I can do. " +
                       $"I'm not a mind reader... yet. 🧠";

            // Default sassy fallbacks
            return PickResponse(new[] {
                "I have NO idea what you just said. And trust me, I tried. Try **skills** to see what I can do?",
                "Hmm, that one's above my pay grade. Try asking something else, or type **help**.",
                "Yeah... I'm gonna need you to rephrase that. My human-to-AI translator is glitching.",
                "I don't know what that means, but it sounds important. Type **help** so I can actually assist.",
                "404: Response not found. But hey, at least I'm honest about it! Try **skills**?",
                "That's outside my expertise. But give me more skills and I'll get there! Type **help** for now."
            });
        }

        private string GetHelpText()
        {
            return "🤖 **Rama — Your Sassy AI Assistant**\n\n" +
                   "**Built-in Commands:**\n" +
                   "• Type or say anything — I'll figure it out\n" +
                   "• `skills` — List all my abilities\n" +
                   "• `help` — This message (congrats, you found it)\n" +
                   "• `what have you learned` — Check my brain stats\n" +
                   "• `remember [fact]` — Teach me something\n" +
                   "• `set sass max` — Unleash the beast 🔥\n" +
                   "• `set sass low` — Calm me down\n\n" +
                   "**Voice Commands:**\n" +
                   "• Click the 🎤 button to talk to me\n" +
                   "• I'll speak my responses back to you\n" +
                   "• Say \"stop listening\" to pause voice mode\n\n" +
                   "**Pro Tips:**\n" +
                   "• I learn from every conversation\n" +
                   "• Add custom skills to make me even smarter\n" +
                   "• The sassier you are, the sassier I get\n" +
                   "• I remember things. Choose your words wisely. 😏";
        }

        /// <summary>
        /// Provide feedback on the last interaction.
        /// </summary>
        public void ProvideFeedback(string input, bool positive)
        {
            _learner.SetFeedback(input, positive ? 1 : -1);
        }

        /// <summary>
        /// Get suggestions based on learned patterns.
        /// </summary>
        public List<string> GetSuggestions()
        {
            return _learner.GetTopPatterns(5);
        }

        /// <summary>
        /// Get learning statistics.
        /// </summary>
        public LearningStats GetStats()
        {
            return _learner.GetStats();
        }

        private string PickResponse(string[] options)
        {
            return options[_rand.Next(options.Length)];
        }
    }
}
