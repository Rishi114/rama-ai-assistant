using System.Net.Http;
using Newtonsoft.Json;

namespace Rama.Skills
{
    /// <summary>
    /// Fetches weather information using the wttr.in API (no API key required).
    /// Supports current weather, forecasts, and weather for specific locations.
    /// </summary>
    public class WeatherSkill : SkillBase
    {
        public override string Name => "Weather";

        public override string Description => "Get current weather and forecasts for any location";

        public override string[] Triggers => new[]
        {
            "weather", "forecast", "temperature", "rain", "snow",
            "is it hot", "is it cold", "is it raining", "what's the weather",
            "how's the weather", "weather in", "weather for", "weather at"
        };

        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        public override bool CanHandle(string input)
        {
            var lower = input.ToLowerInvariant();
            return Triggers.Any(t => lower.Contains(t));
        }

        public override async Task<string> ExecuteAsync(string input, Core.Memory memory)
        {
            var location = ExtractLocation(input);

            if (string.IsNullOrWhiteSpace(location))
            {
                // Try to use a saved location
                location = memory.GetPreference("preferred_location");
            }

            if (string.IsNullOrWhiteSpace(location))
                return "Where would you like to check the weather? " +
                       "Example: \"weather in New York\" or \"weather London\"\n\n" +
                       "You can also set a default: \"set location to San Francisco\"";

            // Handle "set location to X"
            if (input.ToLowerInvariant().Contains("set location") ||
                input.ToLowerInvariant().Contains("my location"))
            {
                var setLocation = ExtractLocation(
                    input.Replace("set location to", "", StringComparison.OrdinalIgnoreCase)
                         .Replace("set my location to", "", StringComparison.OrdinalIgnoreCase)
                         .Replace("my location is", "", StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(setLocation))
                {
                    memory.SetPreference("preferred_location", setLocation);
                    return $"Got it! I've set your default location to **{setLocation}**. " +
                           $"Checking the weather there now...\n\n" +
                           await GetWeatherAsync(setLocation);
                }
            }

            return await GetWeatherAsync(location);
        }

        private async Task<string> GetWeatherAsync(string location)
        {
            try
            {
                // Fetch weather data from wttr.in as JSON
                var url = $"https://wttr.in/{Uri.EscapeDataString(location)}?format=j1";
                var response = await _httpClient.GetStringAsync(url);
                var data = JsonConvert.DeserializeObject<WttrResponse>(response);

                if (data?.Current_Condition == null || data.Current_Condition.Length == 0)
                    return $"Couldn't get weather data for **{location}**. Check the location name.";

                var current = data.Current_Condition[0];
                var nearestArea = data.NearestArea?.FirstOrDefault();

                var locationName = nearestArea != null
                    ? $"{nearestArea.AreaName?.FirstOrDefault()?.Value ?? location}, " +
                      $"{nearestArea.Country?.FirstOrDefault()?.Value ?? ""}"
                    : location;

                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"🌤️ **Weather for {locationName}**\n");

                // Current conditions
                var tempC = current.Temp_C;
                var tempF = current.Temp_F;
                var feelsLikeC = current.FeelsLikeC;
                var feelsLikeF = current.FeelsLikeF;
                var description = current.WeatherDesc?.FirstOrDefault()?.Value ?? "Unknown";
                var humidity = current.Humidity;
                var windSpeedKmph = current.WindspeedKmph;
                var windSpeedMph = current.WindspeedMiles;
                var windDir = current.Winddir16Point;
                var visibility = current.Visibility;
                var pressure = current.Pressure;
                var uvIndex = current.UVIndex;

                sb.AppendLine($"**Current:** {description}");
                sb.AppendLine($"**Temperature:** {tempC}°C / {tempF}°F");
                sb.AppendLine($"**Feels Like:** {feelsLikeC}°C / {feelsLikeF}°F");
                sb.AppendLine($"**Humidity:** {humidity}%");
                sb.AppendLine($"**Wind:** {windSpeedKmph} km/h ({windSpeedMph} mph) {windDir}");
                sb.AppendLine($"**Visibility:** {visibility} km");
                sb.AppendLine($"**Pressure:** {pressure} mb");
                sb.AppendLine($"**UV Index:** {uvIndex}");

                // Forecast (next 3 days)
                if (data.Weather != null && data.Weather.Length > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("**📅 Forecast:**");

                    foreach (var day in data.Weather.Take(3))
                    {
                        var date = day.Date;
                        var maxTempC = day.MaxtempC;
                        var minTempC = day.MintempC;
                        var desc = day.Hourly?.FirstOrDefault()?.WeatherDesc?.FirstOrDefault()?.Value ?? "";
                        var rainChance = day.Hourly?.FirstOrDefault()?.Chanceofrain ?? "0";

                        sb.AppendLine($"  📆 **{date}**: {minTempC}°C — {maxTempC}°C, {desc} (🌧️ {rainChance}% rain)");
                    }
                }

                // Sunrise/Sunset
                if (data.Weather?.FirstOrDefault()?.Astronomy?.FirstOrDefault() is { } astro)
                {
                    sb.AppendLine();
                    sb.AppendLine($"🌅 Sunrise: {astro.Sunrise} | 🌇 Sunset: {astro.Sunset}");
                    sb.AppendLine($"🌙 Moonrise: {astro.Moonrise} | 🌑 Moonset: {astro.Moonset}");
                }

                return sb.ToString();
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                return $"I couldn't find weather data for **\"{location}\"**. " +
                       "Check the spelling or try a more specific location.";
            }
            catch (TaskCanceledException)
            {
                return "The weather service timed out. Please try again in a moment.";
            }
            catch (Exception ex)
            {
                return $"Failed to get weather: {ex.Message}\n\n" +
                       "Try again later or check your internet connection.";
            }
        }

        private string ExtractLocation(string input)
        {
            var lower = input.ToLowerInvariant();

            // Remove trigger phrases
            var patterns = new[]
            {
                "weather in ", "weather for ", "weather at ",
                "forecast in ", "forecast for ",
                "what's the weather in ", "how's the weather in ",
                "what is the weather in ", "what's the weather like in ",
                "temperature in ", "is it hot in ", "is it cold in ",
                "is it raining in ", "weather near ", "weather around "
            };

            foreach (var pattern in patterns)
            {
                var idx = lower.IndexOf(pattern);
                if (idx >= 0)
                    return input.Substring(idx + pattern.Length).Trim();
            }

            // If it just says "weather" by itself, return empty
            if (lower.Trim() == "weather" || lower.Trim() == "forecast" ||
                lower.Trim() == "what's the weather")
                return "";

            // Try to extract: remove the trigger word and use the rest
            foreach (var trigger in Triggers)
            {
                if (lower.StartsWith(trigger + " "))
                    return input.Substring(trigger.Length + 1).Trim();
                if (lower.Contains(trigger + " "))
                {
                    var idx = lower.IndexOf(trigger + " ");
                    return input.Substring(idx + trigger.Length + 1).Trim();
                }
            }

            return "";
        }
    }

    #region wttr.in JSON response models

    public class WttrResponse
    {
        [JsonProperty("current_condition")]
        public WttrCurrentCondition[]? Current_Condition { get; set; }

        [JsonProperty("nearest_area")]
        public WttrNearestArea[]? NearestArea { get; set; }

        [JsonProperty("weather")]
        public WttrWeatherDay[]? Weather { get; set; }
    }

    public class WttrCurrentCondition
    {
        [JsonProperty("temp_C")]
        public string Temp_C { get; set; } = "0";

        [JsonProperty("temp_F")]
        public string Temp_F { get; set; } = "0";

        [JsonProperty("FeelsLikeC")]
        public string FeelsLikeC { get; set; } = "0";

        [JsonProperty("FeelsLikeF")]
        public string FeelsLikeF { get; set; } = "0";

        [JsonProperty("humidity")]
        public string Humidity { get; set; } = "0";

        [JsonProperty("windspeedKmph")]
        public string WindspeedKmph { get; set; } = "0";

        [JsonProperty("windspeedMiles")]
        public string WindspeedMiles { get; set; } = "0";

        [JsonProperty("winddir16Point")]
        public string Winddir16Point { get; set; } = "";

        [JsonProperty("visibility")]
        public string Visibility { get; set; } = "0";

        [JsonProperty("pressure")]
        public string Pressure { get; set; } = "0";

        [JsonProperty("uvIndex")]
        public string UVIndex { get; set; } = "0";

        [JsonProperty("weatherDesc")]
        public WttrDescription[]? WeatherDesc { get; set; }
    }

    public class WttrDescription
    {
        [JsonProperty("value")]
        public string Value { get; set; } = "";
    }

    public class WttrNearestArea
    {
        [JsonProperty("areaName")]
        public WttrDescription[]? AreaName { get; set; }

        [JsonProperty("country")]
        public WttrDescription[]? Country { get; set; }
    }

    public class WttrWeatherDay
    {
        [JsonProperty("date")]
        public string Date { get; set; } = "";

        [JsonProperty("maxtempC")]
        public string MaxtempC { get; set; } = "0";

        [JsonProperty("mintempC")]
        public string MintempC { get; set; } = "0";

        [JsonProperty("hourly")]
        public WttrHourly[]? Hourly { get; set; }

        [JsonProperty("astronomy")]
        public WttrAstronomy[]? Astronomy { get; set; }
    }

    public class WttrHourly
    {
        [JsonProperty("weatherDesc")]
        public WttrDescription[]? WeatherDesc { get; set; }

        [JsonProperty("chanceofrain")]
        public string Chanceofrain { get; set; } = "0";
    }

    public class WttrAstronomy
    {
        [JsonProperty("sunrise")]
        public string Sunrise { get; set; } = "";

        [JsonProperty("sunset")]
        public string Sunset { get; set; } = "";

        [JsonProperty("moonrise")]
        public string Moonrise { get; set; } = "";

        [JsonProperty("moonset")]
        public string Moonset { get; set; } = "";
    }

    #endregion
}
