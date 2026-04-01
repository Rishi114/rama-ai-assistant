using System.Data;

namespace Rama.Skills
{
    /// <summary>
    /// Performs mathematical calculations from natural language input.
    /// Supports basic arithmetic, parentheses, common math functions,
    /// and unit conversions. Uses System.Data.DataTable for safe expression evaluation.
    /// </summary>
    public class CalculatorSkill : SkillBase
    {
        public override string Name => "Calculator";

        public override string Description => "Perform math calculations and unit conversions";

        public override string[] Triggers => new[]
        {
            "calculate", "compute", "math", "what is", "how much",
            "add", "subtract", "multiply", "divide", "percent",
            "square root", "power", "factorial", "convert",
            "sum", "product", "difference"
        };

        // Math constants for extended calculations
        private static readonly Dictionary<string, double> Constants = new()
        {
            { "pi", Math.PI },
            { "e", Math.E },
            { "tau", Math.Tau },
        };

        public override bool CanHandle(string input)
        {
            var lower = input.ToLowerInvariant().Trim();

            // Direct math expression: starts with a number or paren
            if (char.IsDigit(lower[0]) || lower[0] == '(')
            {
                // Check if it looks like a math expression
                var mathChars = "+-*/^%()";
                return lower.Any(c => mathChars.Contains(c)) ||
                       lower.Contains("times") || lower.Contains("divided");
            }

            // Contains explicit math keywords
            if (lower.Contains("calculate") || lower.Contains("compute") ||
                lower.Contains("what is") || lower.Contains("how much is") ||
                lower.Contains("square root") || lower.Contains("percent"))
                return true;

            // Pattern: "X plus/minus/times Y"
            var operationWords = new[] { " plus ", " minus ", " times ", " divided by ",
                                          " to the power of ", " squared", " cubed" };
            return operationWords.Any(op => lower.Contains(op));
        }

        public override Task<string> ExecuteAsync(string input, Core.Memory memory)
        {
            var lower = input.ToLowerInvariant();

            try
            {
                // Handle percentage calculations
                if (lower.Contains("percent") || lower.Contains("%"))
                {
                    return Task.FromResult(CalculatePercentage(input));
                }

                // Handle square root
                if (lower.Contains("square root"))
                {
                    return Task.FromResult(CalculateSquareRoot(input));
                }

                // Handle unit conversion
                if (lower.Contains("convert"))
                {
                    return Task.FromResult(ConvertUnits(input));
                }

                // Handle factorial
                if (lower.Contains("factorial"))
                {
                    return Task.FromResult(CalculateFactorial(input));
                }

                // General calculation
                return Task.FromResult(GeneralCalculate(input));
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    $"I couldn't calculate that: {ex.Message}\n\n" +
                    "Try something like: \"calculate 2 + 2\" or \"what is 15% of 200\"");
            }
        }

        private string GeneralCalculate(string input)
        {
            // Extract the math expression from natural language
            var expression = ExtractExpression(input);

            if (string.IsNullOrWhiteSpace(expression))
                return "I need a math expression to calculate. " +
                       "Example: \"calculate (15 + 3) * 2\" or \"what is 100 / 4\"";

            // Replace word operations with symbols
            expression = NormalizeExpression(expression);

            // Safe evaluation using DataTable
            var result = EvaluateExpression(expression);

            return FormatResult(expression, result);
        }

        private string ExtractExpression(string input)
        {
            var lower = input.ToLowerInvariant();

            // Remove prefixes
            var prefixes = new[] { "calculate", "compute", "what is", "how much is",
                                   "what's", "solve", "evaluate" };
            var expr = input;
            foreach (var prefix in prefixes)
            {
                if (lower.StartsWith(prefix + " "))
                {
                    expr = input.Substring(prefix.Length + 1).Trim();
                    break;
                }
            }

            // Replace word operations
            expr = expr.Replace("plus", "+", StringComparison.OrdinalIgnoreCase)
                       .Replace("add", "+", StringComparison.OrdinalIgnoreCase)
                       .Replace("minus", "-", StringComparison.OrdinalIgnoreCase)
                       .Replace("subtract", "-", StringComparison.OrdinalIgnoreCase)
                       .Replace("times", "*", StringComparison.OrdinalIgnoreCase)
                       .Replace("multiplied by", "*", StringComparison.OrdinalIgnoreCase)
                       .Replace("multiply", "*", StringComparison.OrdinalIgnoreCase)
                       .Replace("divided by", "/", StringComparison.OrdinalIgnoreCase)
                       .Replace("divide", "/", StringComparison.OrdinalIgnoreCase)
                       .Replace("to the power of", "^", StringComparison.OrdinalIgnoreCase)
                       .Replace("raised to", "^", StringComparison.OrdinalIgnoreCase)
                       .Replace("squared", "^2", StringComparison.OrdinalIgnoreCase)
                       .Replace("cubed", "^3", StringComparison.OrdinalIgnoreCase);

            return expr.Trim();
        }

        private string NormalizeExpression(string expression)
        {
            // Replace ^ with Math.Pow equivalent (DataTable doesn't support ^)
            var result = expression;

            // Handle power operations: convert X^Y to a calculated approach
            while (result.Contains('^'))
            {
                var idx = result.IndexOf('^');
                // Find the base (number or paren group before ^)
                var baseStart = idx - 1;
                while (baseStart >= 0 && (char.IsDigit(result[baseStart]) ||
                       result[baseStart] == '.' || result[baseStart] == ')'))
                    baseStart--;
                baseStart++;

                // Find the exponent
                var expStart = idx + 1;
                var expEnd = expStart;
                while (expEnd < result.Length && (char.IsDigit(result[expEnd]) ||
                       result[expEnd] == '.' || result[expEnd] == '('))
                    expEnd++;

                if (baseStart < idx && expStart < expEnd)
                {
                    var baseStr = result.Substring(baseStart, idx - baseStart);
                    var expStr = result.Substring(expStart, expEnd - expStart);

                    if (double.TryParse(baseStr, out var baseVal) &&
                        double.TryParse(expStr, out var expVal))
                    {
                        var powResult = Math.Pow(baseVal, expVal);
                        result = result.Substring(0, baseStart) + powResult +
                                 result.Substring(expEnd);
                    }
                    else
                    {
                        break; // Can't parse, give up
                    }
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private double EvaluateExpression(string expression)
        {
            // Use DataTable.Compute for safe arithmetic evaluation
            var table = new DataTable();
            var result = table.Compute(expression, null);
            return Convert.ToDouble(result);
        }

        private string FormatResult(string expression, double result)
        {
            var formattedResult = result == Math.Floor(result)
                ? result.ToString("F0")
                : result.ToString("F6").TrimEnd('0').TrimEnd('.');

            return $"🧮 **{expression}** = **{formattedResult}**";
        }

        private string CalculatePercentage(string input)
        {
            var lower = input.ToLowerInvariant();

            // Pattern: "X percent of Y" or "X% of Y"
            var match = System.Text.RegularExpressions.Regex.Match(lower,
                @"(\d+(?:\.\d+)?)\s*(?:percent|%)\s*of\s*(\d+(?:\.\d+)?)");

            if (match.Success)
            {
                var percent = double.Parse(match.Groups[1].Value);
                var value = double.Parse(match.Groups[2].Value);
                var result = (percent / 100) * value;
                var formatted = result == Math.Floor(result) ? result.ToString("F0") : result.ToString("F2");
                return $"🧮 {percent}% of {value} = **{formatted}**";
            }

            // Pattern: "what is X% of Y" — same pattern, try extracting
            match = System.Text.RegularExpressions.Regex.Match(lower,
                @"(\d+(?:\.\d+)?)%\s*of\s*(\d+(?:\.\d+)?)");
            if (match.Success)
            {
                var percent = double.Parse(match.Groups[1].Value);
                var value = double.Parse(match.Groups[2].Value);
                var result = (percent / 100) * value;
                var formatted = result == Math.Floor(result) ? result.ToString("F0") : result.ToString("F2");
                return $"🧮 {percent}% of {value} = **{formatted}**";
            }

            return "I couldn't parse the percentage. Try: \"what is 15% of 200\"";
        }

        private string CalculateSquareRoot(string input)
        {
            var match = System.Text.RegularExpressions.Regex.Match(input, @"(\d+(?:\.\d+)?)");
            if (match.Success && double.TryParse(match.Groups[1].Value, out var num))
            {
                if (num < 0)
                    return "Can't take the square root of a negative number!";

                var result = Math.Sqrt(num);
                return $"🧮 √{num} = **{result:F6}".TrimEnd('0').TrimEnd('.') + "**";
            }

            return "What number? Example: \"square root of 144\"";
        }

        private string CalculateFactorial(string input)
        {
            var match = System.Text.RegularExpressions.Regex.Match(input, @"(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var num))
            {
                if (num < 0)
                    return "Factorial is not defined for negative numbers!";
                if (num > 170)
                    return "That number is too large for factorial computation!";

                double result = 1;
                for (int i = 2; i <= num; i++)
                    result *= i;

                return $"🧮 {num}! = **{result:F0}**";
            }

            return "What number? Example: \"factorial of 10\"";
        }

        private string ConvertUnits(string input)
        {
            // Temperature
            if (input.ToLowerInvariant().Contains("fahrenheit") && input.ToLowerInvariant().Contains("celsius"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(input, @"(\d+(?:\.\d+)?)");
                if (match.Success && double.TryParse(match.Groups[1].Value, out var temp))
                {
                    if (input.ToLowerInvariant().Contains("to celsius") ||
                        input.ToLowerInvariant().Contains("in celsius"))
                    {
                        var celsius = (temp - 32) * 5 / 9;
                        return $"🌡️ {temp}°F = **{celsius:F1}°C**";
                    }
                    var fahrenheit = (temp * 9 / 5) + 32;
                    return $"🌡️ {temp}°C = **{fahrenheit:F1}°F**";
                }
            }

            // Length
            if (input.ToLowerInvariant().Contains("km") && input.ToLowerInvariant().Contains("mile"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(input, @"(\d+(?:\.\d+)?)");
                if (match.Success && double.TryParse(match.Groups[1].Value, out var val))
                {
                    if (input.ToLowerInvariant().Contains("to mile"))
                    {
                        var miles = val * 0.621371;
                        return $"📏 {val} km = **{miles:F2} miles**";
                    }
                    var km = val / 0.621371;
                    return $"📏 {val} miles = **{km:F2} km**";
                }
            }

            // Weight
            if (input.ToLowerInvariant().Contains("kg") && input.ToLowerInvariant().Contains("pound"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(input, @"(\d+(?:\.\d+)?)");
                if (match.Success && double.TryParse(match.Groups[1].Value, out var val))
                {
                    if (input.ToLowerInvariant().Contains("to pound"))
                    {
                        var lbs = val * 2.20462;
                        return $"⚖️ {val} kg = **{lbs:F2} lbs**";
                    }
                    var kg = val / 2.20462;
                    return $"⚖️ {val} lbs = **{kg:F2} kg**";
                }
            }

            return "Supported conversions:\n" +
                   "• Temperature: \"convert 100 F to Celsius\"\n" +
                   "• Distance: \"convert 10 km to miles\"\n" +
                   "• Weight: \"convert 50 kg to pounds\"";
        }
    }
}
