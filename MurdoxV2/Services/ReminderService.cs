using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Services
{
    public class ReminderService(IDbContextFactory<AppDbContext> dbFactory) : IReminder
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

        #region SAVE REMINDER
        /// <summary>
        /// Save reminder to the database
        /// </summary>
        /// <param name="reminder"></param>
        /// <returns>bool success/fail</returns>
        public async Task<Result<bool, SystemError<ReminderService>>> SaveReminderAsync(Reminder reminder)
        {
            using var db = _dbFactory.CreateDbContext();
            var member = await db.Members
                .FirstOrDefaultAsync(m => m.DiscordId == reminder.DiscordId && m.GuildId == reminder.GuildId);
            if (member is null)
            {
                member = new ServerMember()
                {
                    DiscordId = reminder.Member.DiscordId,
                    GuildId = reminder.GuildId,
                    GlobalUsername = reminder.Member.GlobalUsername,
                    Discriminator = reminder.Member.Discriminator,
                    Nickname = reminder.Member.Nickname,
                    AvatarUrl = reminder.Member.AvatarUrl,
                    JoinedAt = DateTimeOffset.UtcNow.ToUniversalTime(),
                    CreatedAt = DateTimeOffset.UtcNow.ToUniversalTime(),
                    IsBot = reminder.Member.IsBot,
                    IsMuted = reminder.Member.IsMuted,
                    IsBanned = reminder.Member.IsBanned,
                    MessageCount = reminder.Member.MessageCount,
                    Bank = reminder.Member.Bank ?? new Bank()
                    {
                        Balance = 100,
                        Deposit_Amount = 0,
                        Deposit_Timestamp = DateTime.UtcNow,
                    }
                };
                await db.Members.AddAsync(member);
                await db.SaveChangesAsync();
            }
            
            await db.AddAsync(reminder);
            try
            {
                await db.SaveChangesAsync();
                return Result<bool, SystemError<ReminderService>>.Ok(true);
            }
            catch (DbUpdateException ex)
            {
                Log.Error(ex, "Error saving reminder to the database.");
                return Result<bool, SystemError<ReminderService>>.Err(new SystemError<ReminderService>
                {
                    ErrorMessage = ex.Message,
                    ErrorType = Enums.ErrorType.INFORMATION,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = this,
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unexpected error occurred while saving the reminder.");
                return Result<bool, SystemError<ReminderService>>.Err(new SystemError<ReminderService>
                {
                    ErrorMessage = ex.Message,
                    ErrorType = Enums.ErrorType.INFORMATION,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = this,
                });

            }
        }
        #endregion

        #region GET ALL REMINDERS FOR GUILD
        public async Task<Result<List<Reminder>, SystemError<ReminderService>>> GetAllRemindersAsync()
        {
            using var db = _dbFactory.CreateDbContext();
            var reminders = await db.Reminders
                .ToListAsync();
            if (reminders.Count > 0)
                return Result<List<Reminder>, SystemError<ReminderService>>.Ok(reminders);
            else
                return Result<List<Reminder>, SystemError<ReminderService>>.Err(new SystemError<ReminderService>
                {
                    ErrorMessage = "No reminders found for this guild.",
                    ErrorType = Enums.ErrorType.INFORMATION,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = this,
                });

        }
        #endregion

        #region UPDATE REMINDER
        public async Task<Result<bool, SystemError<ReminderService>>> UpdateMemberReminderAsync(Reminder reminder)
        {
            using var db = _dbFactory.CreateDbContext();
            var existingReminder = await db.Reminders.FindAsync(reminder.Id);
            if (existingReminder is not null)
            {
                existingReminder.IsComplete = true;
                db.Update(existingReminder);
                await db.SaveChangesAsync();
                return Result<bool, SystemError<ReminderService>>.Ok(true);
            }
            return Result<bool, SystemError<ReminderService>>.Err(new SystemError<ReminderService>
            {
                ErrorMessage = "Could not update reminder.",
                ErrorType = Enums.ErrorType.INFORMATION,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = this,
            });
        }
        #endregion
    }
}
