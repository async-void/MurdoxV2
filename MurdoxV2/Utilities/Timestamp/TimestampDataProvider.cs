using Humanizer;
using System.Text.RegularExpressions;

namespace MurdoxV2.Utilities.Timestamp
{
    public partial class TimestampDataProvider
    {
        public static void SetBotTimestamp()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "uptime.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public static async Task<string> GetBotUptimeAsync()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "uptime.txt");
            if (!File.Exists(path))
            {
                return "Uptime data not found.";
            }
            var uptime = await File.ReadAllTextAsync(path);
            if (DateTime.TryParse(uptime, out DateTime utime))
            {
                var duration = DateTime.UtcNow - utime;
                return $"**{duration.Humanize(3, maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second)}**";
            }
            else
            {
                return "Invalid uptime data format.";
            }
        }

        #region PARSE TIMEOUT DURATION
        public static DateTimeOffset ParseTimeout(int value, char unit)
        {
            TimeSpan timeout = unit switch
            {
                's' => TimeSpan.FromSeconds(value),
                'm' => TimeSpan.FromMinutes(value),
                'h' => TimeSpan.FromHours(value),
                'd' => TimeSpan.FromDays(value),
                'w' => TimeSpan.FromDays(value * 7),
                _ => throw new ArgumentException($"Unknown duration unit '{unit}'")
            };

            return DateTimeOffset.UtcNow.Add(timeout);
        }

        #endregion

        #region TRY VALIDATE TIMEOUT
        public static (bool Success, int Value, char Unit) ValidateTimeout(string input)
        {
            var match = Timeout().Match(input);

            if (!match.Success)
                return (false, 0, '\0');

            int value = int.Parse(match.Groups["value"].Value);
            char unit = char.ToLowerInvariant(match.Groups["unit"].Value[0]);

            return (true, value, unit);
        }


        [GeneratedRegex(@"^(?<value>\d+)(?<unit>[smhdw])$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
        private static partial Regex Timeout();
        #endregion
    }
}
