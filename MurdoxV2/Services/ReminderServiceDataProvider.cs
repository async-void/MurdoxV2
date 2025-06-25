using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using System.Text.RegularExpressions;

namespace MurdoxV2.Services
{
    public class ReminderServiceDataProvider(IDbContextFactory<AppDbContext> dbFactory) : IReminderData
    {
        public Task<Result<bool, SystemError<ReminderDataServiceProvider>>> BulkDeleteMemberRemindersAsync(string guildId, string discordId, List<Reminder> reminders)
        {
            throw new NotImplementedException();
        }

        public Task<Result<bool, SystemError<ReminderDataServiceProvider>>> DeleteMemberReminderAsync(string guildId, string discordId, int reminderId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<Reminder>, SystemError<ReminderDataServiceProvider>>> GetMemberRemindersAsync(string guildId, string discordId)
        {
            throw new NotImplementedException();
        }

        public Result<int, SystemError<ReminderDataServiceProvider>> ParseTimeString(string input)
        {
            var totalMilliseconds = 0;
            var matches = Regex.Matches(input, @"(\d+)\s*([dhms])", RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                if (match.Groups.Count != 3) continue;
                if (!int.TryParse(match.Groups[1].Value, out int value))
                    continue;
                var unit = match.Groups[2].Value.ToLowerInvariant();
                switch (unit)
                {
                    case "d":
                        totalMilliseconds += value * 24 * 60 * 60 * 1000; // Days to milliseconds
                        break;
                    case "h":
                        totalMilliseconds += value * 60 * 60 * 1000; // Hours to milliseconds
                        break;
                    case "m":
                        totalMilliseconds += value * 60 * 1000; // Minutes to milliseconds
                        break;
                    case "s":
                        totalMilliseconds += value * 1000; // Seconds to milliseconds
                        break;
                }
            }
            return Result<int, SystemError<ReminderDataServiceProvider>>.Ok(totalMilliseconds);
        }

        public async Task<Result<bool, SystemError<ReminderDataServiceProvider>>> SaveMemberRemindersAsync(string guildId, string discordId, Reminder reminder)
        {
            using var db = dbFactory.CreateDbContext();
            var savedReminder = await db.Reminders.FirstOrDefaultAsync(r => r.Member!.DiscordId.Equals(discordId) 
                                    && r.GuildId.Equals(guildId) && r.Content.Equals(reminder.Content));

            if (savedReminder is not null)
                return Result<bool, SystemError<ReminderDataServiceProvider>>.Err(
                    new SystemError<ReminderDataServiceProvider>
                    {
                        ErrorMessage = "Reminder Already Exists",
                        ErrorType = Enums.ErrorType.WARNING,
                        CreatedAt = DateTime.UtcNow,
                    });
            await db.Reminders.AddAsync(reminder);
            await db.SaveChangesAsync();
            return Result<bool, SystemError<ReminderDataServiceProvider>>.Ok(true);
        }
    }
}
