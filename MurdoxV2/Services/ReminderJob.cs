using DSharpPlus;
using Humanizer;
using Microsoft.Extensions.Logging;
using MurdoxV2.Interfaces;
using Quartz;
using Serilog;

namespace MurdoxV2.Services
{
    public class ReminderJob(IReminder reminderService, DiscordClient client, ILogger<ReminderJob> logger) : IJob
    {
        private readonly DiscordClient _client = client;
        private readonly IReminder _reminderService = reminderService;
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                logger.LogInformation($"fetching reminders from database...");
                var reminders = await _reminderService.GetAllRemindersAsync();
               
                if (reminders.IsOk)
                {
                    
                    foreach (var reminder in reminders.Value)
                    {
                        var isDue = DateTimeOffset.UtcNow.CompareTo(reminder.CompleteAt);
                        var totalTime = reminder.CreatedAt;
                        var duration = totalTime.Humanize();
                        var completed = reminder.IsComplete;
                        var channelId = reminder.ChannelId;
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
                logger.LogError("Error executing reminder - Error: {ex}", ex);
            }
            
        }
    }
}
