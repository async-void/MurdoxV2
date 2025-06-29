using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Interfaces;
using Quartz;
using Serilog;

namespace MurdoxV2.Services
{
    public class SchedulerService(IReminder reminderService) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var reminders = await reminderService.GetAllRemindersForGuildAsync("764184337620140062");
                if (reminders.IsOk)
                    Log.Information($"fetching reminders from database... {reminders.Value.Count} reminders in db");
                else
                    Log.Error($"Error fetching reminders: {reminders.Error.ErrorMessage}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error executing reminder job");
            }
            
        }
    }
}
