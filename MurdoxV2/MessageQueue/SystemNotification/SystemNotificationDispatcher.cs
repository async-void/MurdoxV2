using DSharpPlus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections;

namespace MurdoxV2.MessageQueue.SystemNotification
{
    public class SystemNotificationDispatcher(DiscordClient client, ISystemNotificationQueue queue, ILogger<SystemNotificationDispatcher> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            logger.LogInformation("SystemNotificationDispatcher started");

            while (!ct.IsCancellationRequested)
            {
                if (!queue.TryDequeue(out var payload) || payload is null)
                {
                    await Task.Delay(200, ct);
                    continue;
                }

                if (payload.GuildId == 0)
                {
                    // Broadcast to all guilds
                    foreach (var guild in client.Guilds.Values)
                    {
                        if (ct.IsCancellationRequested)
                            return;

                        await SendToGuildAsync(guild.Id, payload, ct);
                        await Task.Delay(1000, ct);
                    }
                }
                else
                {
                    // Targeted notification
                    await SendToGuildAsync(payload.GuildId, payload, ct);
                }
            }
        }

        private async Task SendToGuildAsync(ulong guildId, SystemNotificationPayload payload, CancellationToken ct)
        {
            try
            {
                var guild = await client.GetGuildAsync(guildId);
                if (guild is null)
                    return;

                var channelId = await GetNotificationChannelForGuild(guildId);
                if (channelId is null)
                    return;

                var channel = await guild.GetChannelAsync(channelId.Value);
                if (channel is null)
                    return;

                await channel.SendMessageAsync(payload.Message);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send system notification to guild {GuildId}", guildId);
            }
        }

        private async Task<ulong?> GetNotificationChannelForGuild(ulong guildId)
        {
            // TODO: Replace with your real config lookup
            var guild = await client.GetGuildAsync(guildId);
            var channel = guild?.GetDefaultChannel();
            return channel?.Id ?? 0;
        }
    }
}
