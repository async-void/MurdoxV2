using Humanizer;
using Humanizer.Localisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Utilities.Timestamp
{
    public class TimestampDataProvider
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
    }
}
