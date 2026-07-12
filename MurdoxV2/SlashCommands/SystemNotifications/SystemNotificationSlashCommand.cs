using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using MurdoxV2.MessageQueue.SystemNotification;
using MurdoxV2.RoleCheck;
using MurdoxV2.Services.Authorization;
using System.ComponentModel;

namespace MurdoxV2.SlashCommands.SystemNotifications
{
    public class SystemNotificationSlashCommand(ISystemNotificationQueue _queue, ILogger<SystemNotificationSlashCommand> _logger,
        RoleAuthorizationService authService)
    {
        private readonly RoleAuthorizationService _authService = authService;

        #region SYSTEM NOTIFICATION
        [Command("broadcast")]
        [Description("Send a system-wide notification.")]
        [SystemNotificationRole(DiscordPermission.Administrator)]
        public async ValueTask Broadcast(SlashCommandContext ctx, [Parameter("message")] string message)
        {
            await ctx.DeferResponseAsync(true);
            var callingMember = await ctx.Guild!.GetMemberAsync(ctx.User.Id);
            if (!await _authService.IsAuthorizedAsync(callingMember))
            {
                await ctx.RespondAsync("❗ You are not authorized to use this command.");
                _logger.LogInformation("member [{member}] attempted to execute the System Notification command in [{channel}] for [{guild}], but failed authorization.", callingMember.DisplayName, ctx.Channel.Name, ctx.Guild.Name);
                return;
            }
            var payload = new SystemNotificationPayload
            {
                Message = message,
                Author = ctx.Interaction.User.Id,
                GuildId = 0,
                EnqueuedAt = DateTimeOffset.UtcNow,
            };
            _queue.Enqueue(payload);

            await ctx.RespondAsync($"System Notification Message has beed enqued with ID: {payload.Id}");
            _logger.LogInformation("member [{member}] executed the System Notification command in [{channel}] for [{guild}].", callingMember.DisplayName, ctx.Channel.Name, ctx.Guild.Name);
        }
        #endregion
    }
}
