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
        Result<int, SystemError<ReminderServiceDataProvider>> ParseTimeString(string input);
        Task<Result<List<Reminder>, SystemError<ReminderServiceDataProvider>>> GetMemberRemindersAsync(string guildId, string discordId);
        Task<Result<bool, SystemError<ReminderServiceDataProvider>>> SaveMemberRemindersAsync(string guildId, string discordId, Reminder reminder);
        Task<Result<bool, SystemError<ReminderServiceDataProvider>>> DeleteMemberReminderAsync(string guildId, string discordId, int reminderId);
        Task<Result<bool, SystemError<ReminderServiceDataProvider>>> BulkDeleteMemberRemindersAsync(string guildId, string discordId, List<Reminder> reminders);
        Task<Result<bool, SystemError<ReminderService>>> UpdateMemberReminderAsync(Reminder reminder);
    }
}
