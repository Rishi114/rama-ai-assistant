using System;
using System.Threading.Tasks;
using Rama.Core;

namespace Rama.Skills
{
    /// <summary>
    /// Profile Skill — Change Rama's name, set your nickname, customize personality.
    /// </summary>
    public class ProfileSkill : SkillBase
    {
        private readonly ProfileManager _profile;

        public ProfileSkill(ProfileManager profile)
        {
            _profile = profile;
        }

        public override string Name => "Profile";
        public override string Description => "Change names, nicknames, personality";
        public override string[] Triggers => new[] {
            "change your name", "rename you", "call you",
            "call me", "my name is", "what's my name",
            "who am i", "your name", "be sassy", "be cute",
            "be naughty", "be professional", "be chill",
            "show profile", "my profile", "settings"
        };

        public override bool CanHandle(string input)
        {
            string lower = input.ToLowerInvariant();
            return lower.Contains("change your name") || lower.Contains("rename") ||
                   lower.Contains("call me") || lower.Contains("my name is") ||
                   lower.Contains("who am i") || lower.Contains("your name") ||
                   lower.Contains("be sassy") || lower.Contains("be cute") ||
                   lower.Contains("be naughty") || lower.Contains("be professional") ||
                   lower.Contains("profile") || lower.Contains("who are you");
        }

        public override Task<string> ExecuteAsync(string input, Memory memory)
        {
            string lower = input.ToLowerInvariant();

            // Change Rama's name
            if (lower.Contains("change your name") || lower.Contains("rename you") || lower.Contains("call you"))
            {
                string newName = System.Text.RegularExpressions.Regex.Replace(
                    input, @"change your name to\s+|rename you to\s+|call you\s+|i'll call you\s+",
                    "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();

                if (string.IsNullOrEmpty(newName))
                    return Task.FromResult($"What do you want to call me? Say `change your name to [name]`");

                return Task.FromResult(_profile.SetBotName(newName));
            }

            // Set user's nickname
            if (lower.Contains("call me"))
            {
                string nickname = System.Text.RegularExpressions.Regex.Replace(
                    input, @"call me\s+", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();

                if (string.IsNullOrEmpty(nickname))
                    return Task.FromResult("What should I call you? Options: bro, bhai, boss, sir ji, brother, yaar, dude, chief, captain, king");

                return Task.FromResult(_profile.SetUserNickname(nickname));
            }

            // Set user's name
            if (lower.Contains("my name is"))
            {
                string name = System.Text.RegularExpressions.Regex.Replace(
                    input, @"my name is\s+", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();

                if (string.IsNullOrEmpty(name))
                    return Task.FromResult("What's your name? Say `my name is [name]`");

                return Task.FromResult(_profile.SetUserName(name));
            }

            // Who am I
            if (lower.Contains("who am i") || lower.Contains("what's my name"))
            {
                string nick = _profile.GetUserAddress();
                return Task.FromResult($"You're **{_profile.UserName ?? "my favorite human"}**, " +
                    $"and I call you **{nick}**! 😊");
            }

            // Who are you
            if (lower.Contains("who are you") || lower.Contains("your name"))
            {
                return Task.FromResult($"I'm **{_profile.BotName}**! " +
                    $"Your {_profile.BotPersonality} AI assistant. " +
                    $"I call you **{_profile.GetUserAddress()}**. 💪");
            }

            // Change personality
            if (lower.Contains("be sassy"))
                return Task.FromResult(_profile.SetPersonality("sassy"));
            if (lower.Contains("be cute"))
                return Task.FromResult(_profile.SetPersonality("cute"));
            if (lower.Contains("be naughty"))
                return Task.FromResult(_profile.SetPersonality("naughty"));
            if (lower.Contains("be professional"))
                return Task.FromResult(_profile.SetPersonality("professional"));
            if (lower.Contains("be chill"))
                return Task.FromResult(_profile.SetPersonality("chill"));

            // Show profile
            if (lower.Contains("profile") || lower.Contains("who am i"))
                return Task.FromResult(_profile.GetProfileInfo());

            return Task.FromResult(_profile.GetGreeting());
        }
    }
}
