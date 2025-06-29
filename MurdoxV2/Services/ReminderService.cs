using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
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
            await db.AddAsync(reminder);
            try
            {
                await db.SaveChangesAsync();
                return Result<bool, SystemError<ReminderService>>.Ok(true);
            }
            catch (DbUpdateException ex)
            {
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
        public async Task<Result<List<Reminder>, SystemError<ReminderService>>> GetAllRemindersForGuildAsync(string guildId)
        {
            using var db = _dbFactory.CreateDbContext();
          
            var reminders = await db.Reminders
                .Where(r => r.GuildId.Equals(guildId))
                .ToListAsync();
            if (reminders.Count > 0)
                return Result<List<Reminder>, SystemError<ReminderService>>.Ok(reminders);
            else
                return Result<List<Reminder>, SystemError<ReminderService>>.Err(new SystemError<ReminderService>
                {
                    ErrorMessage = "No reminders found for this guild.",
                    ErrorType = Enums.ErrorType.INFORMATION,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = this,
                });

        }
        #endregion
    }
}
