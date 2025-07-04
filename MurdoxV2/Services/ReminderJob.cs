using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Interfaces;
using Quartz;
using Serilog;
using System.Globalization;

namespace MurdoxV2.Services
{
    public class ReminderJob(IReminder reminderService, DiscordClient client) : IJob
    {
        private readonly DiscordClient _client = client;
        private readonly IReminder _reminderService = reminderService;
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Log.Information($"fetching reminders from database...");
                var reminders = await _reminderService.GetAllRemindersAsync();
               
                if (reminders.IsOk)
                {
                    
                    foreach (var reminder in reminders.Value)
                    {
                        var isDue = DateTimeOffset.UtcNow.CompareTo(reminder.CompleteAt);
                        var totalTime = reminder.CreatedAt;
                        var duration = totalTime.Humanize();
                        var completed = reminder.IsComplete;
                        var channelId = ulong.Parse(reminder.ChannelId);
                        var channel = await _client.GetChannelAsync(channelId);

                        if (isDue >= 0)
                        {
                            if(completed)
                            {
                                continue;
                            }
                            else
                            {
                                await _client.SendMessageAsync(channel, $"<@{reminder.DiscordId}> you ask me to remind you ``{duration}`` to ``{reminder.Content}``");
                                var success = await _reminderService.UpdateMemberReminderAsync(reminder);
                                continue;
                            }
                        }                   
                    }
                }  

            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error executing reminder - Error: {ex.Message}");
            }
            
        }
    }
}
