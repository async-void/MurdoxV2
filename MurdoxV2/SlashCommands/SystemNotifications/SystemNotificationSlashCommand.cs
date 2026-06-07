using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using MurdoxV2.MessageQueue.SystemNotification;
using MurdoxV2.RoleCheck;
using System.ComponentModel;

namespace MurdoxV2.SlashCommands.SystemNotifications
{
    public class SystemNotificationSlashCommand(ISystemNotificationQueue _queue, ILogger<SystemNotificationSlashCommand> _logger)
    {
        #region SYSTEM NOTIFICATION
        [Command("broadcast")]
        [Description("Send a system-wide notification.")]
        [SysteNotificationRole(1501498012822933520)]
        public async ValueTask Broadcast(SlashCommandContext ctx, [Parameter("message")] string message)
        {
            await ctx.DeferResponseAsync(true);
            var payload = new SystemNotificationPayload
            {
                Message = message,
                Author = ctx.Interaction.User.Id,
                GuildId = 0,
                EnqueuedAt = DateTimeOffset.UtcNow,
            };
            _queue.Enqueue(payload);

            await ctx.RespondAsync($"System Notification Message has beed enqued with ID: {payload.Id}");
        }
        #endregion
    }
}
