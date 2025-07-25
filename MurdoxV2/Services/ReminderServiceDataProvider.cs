﻿using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Enums;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using System.Text.RegularExpressions;

namespace MurdoxV2.Services
{
    public class ReminderServiceDataProvider(IDbContextFactory<AppDbContext> dbFactory) : IReminderData
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

        #region BULK DELETE MEMBER REMINDERS
        public Task<Result<bool, SystemError<ReminderServiceDataProvider>>> BulkDeleteMemberRemindersAsync(string guildId, string discordId, List<Reminder> reminders)
        {
            throw new NotImplementedException();
        } 
        #endregion

        #region DELETE MEMBER REMINDER
        public Task<Result<bool, SystemError<ReminderServiceDataProvider>>> DeleteMemberReminderAsync(string guildId, string discordId, int reminderId)
        {
            throw new NotImplementedException();
        } 
        #endregion

        #region GET MEMBER REMINDERS
        public async Task<Result<List<Reminder>, SystemError<ReminderServiceDataProvider>>> GetMemberRemindersAsync(string guildId, string discordId)
        {
            var db = _dbFactory.CreateDbContext();
            var reminders = await db.Reminders.Where(m => m.DiscordId.Equals(discordId) && m.GuildId.Equals(guildId)).ToListAsync();
            if (reminders.Count > 0)
                return Result<List<Reminder>, SystemError<ReminderServiceDataProvider>>.Ok(reminders);

            return Result<List<Reminder>, SystemError<ReminderServiceDataProvider>>.Err(new SystemError<ReminderServiceDataProvider>
            {
                ErrorMessage = "No reminders found for this member.",
                ErrorType = Enums.ErrorType.INFORMATION,
                CreatedAt = DateTime.UtcNow,
            });
        }
        #endregion

        #region UPDATE REMINDER
        public async Task<Result<bool, SystemError<ReminderService>>> UpdateMemberReminderAsync(Reminder reminder)
        {
            var db = _dbFactory.CreateDbContext();
            var r = await db.Reminders.FindAsync(reminder.Id);
            if (r is not null)
            {
                r.IsComplete = true;
                db.Update(r);
                await db.SaveChangesAsync();
                return Result<bool, SystemError<ReminderService>>.Ok(true);
            }
            return Result<bool, SystemError<ReminderService>>.Err(new SystemError<ReminderService>
            {
                ErrorMessage = "could not update reminder.",
                ErrorType = ErrorType.INFORMATION,
                CreatedAt = DateTimeOffset.UtcNow,
            });
        }
        #endregion

        #region PARSE TIME STRING
        public Result<int, SystemError<ReminderServiceDataProvider>> ParseTimeString(string input)
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
                    case "y":
                        totalMilliseconds += value * 365 * 24 * 60 * 60 * 1000; // Years to milliseconds
                        break;
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
            return Result<int, SystemError<ReminderServiceDataProvider>>.Ok(totalMilliseconds);
        }
        #endregion

        #region SAVE MEMBER REMINDER
        public async Task<Result<bool, SystemError<ReminderServiceDataProvider>>> SaveMemberRemindersAsync(string guildId, string discordId, Reminder reminder)
        {
            using var db = _dbFactory.CreateDbContext();
            var savedReminder = db.Reminders.Where(r => r.DiscordId.Equals(discordId)
                                    && r.GuildId.Equals(guildId) && r.Content.Equals(reminder.Content)).ToList();

            if (savedReminder is not null)
                return Result<bool, SystemError<ReminderServiceDataProvider>>.Err(
                    new SystemError<ReminderServiceDataProvider>
                    {
                        ErrorMessage = "Reminder Already Exists",
                        ErrorType = Enums.ErrorType.WARNING,
                        CreatedAt = DateTime.UtcNow,
                    });
            await db.Reminders.AddAsync(reminder);
            await db.SaveChangesAsync();
            return Result<bool, SystemError<ReminderServiceDataProvider>>.Ok(true);
        } 
        #endregion
    }
}
