using MurdoxV2.Models;
using MurdoxV2.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Interfaces
{
    public interface IReminder
    {
        Task<Result<bool, SystemError<ReminderService>>> SaveReminderAsync(Reminder reminder);
        Task<Result<List<Reminder>, SystemError<ReminderService>>> GetAllRemindersForGuildAsync(string guildId);
    }
}
