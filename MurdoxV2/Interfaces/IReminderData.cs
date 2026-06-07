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
        Task<Result<List<Reminder>, SystemError<ReminderServiceDataProvider>>> GetMemberRemindersAsync(ulong guildId, ulong discordId);
        Task<Result<bool, SystemError<ReminderServiceDataProvider>>> SaveMemberRemindersAsync(ulong guildId, ulong discordId, Reminder reminder);
        Task<Result<bool, SystemError<ReminderServiceDataProvider>>> DeleteMemberReminderAsync(ulong guildId, ulong discordId, int reminderId);
        Task<Result<bool, SystemError<ReminderServiceDataProvider>>> BulkDeleteMemberRemindersAsync(ulong guildId, ulong discordId, List<Reminder> reminders);
        Task<Result<bool, SystemError<ReminderService>>> UpdateMemberReminderAsync(Reminder reminder);
        List<List<Reminder>> ChunkReminders(List<Reminder> reminders, int chunkSize);
    }
}
