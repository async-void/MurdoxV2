using MurdoxV2.Models;
using MurdoxV2.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Interfaces
{
    public interface IReminderData
    {
        Result<int, SystemError<ReminderDataServiceProvider>> ParseTimeString(string input);
        Task<Result<List<Reminder>, SystemError<ReminderDataServiceProvider>>> GetMemberRemindersAsync(string guildId, string discordId);
        Task<Result<bool, SystemError<ReminderDataServiceProvider>>> SaveMemberRemindersAsync(string guildId, string discordId, Reminder reminder);
        Task<Result<bool, SystemError<ReminderDataServiceProvider>>> DeleteMemberReminderAsync(string guildId, string discordId, int reminderId);
        Task<Result<bool, SystemError<ReminderDataServiceProvider>>> BulkDeleteMemberRemindersAsync(string guildId, string discordId, List<Reminder> reminders);

    }
}
