using DSharpPlus;
using DSharpPlus.EventArgs;
using MurdoxV2.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Services.MessageCache
{
    public sealed class GhostPingService(DiscordMessageCacheService cache, RateLimitHelper<ulong> rateLimitHelper)
    {
        #region SINGLE DELETE
        public async Task HandleSingleDeleteAsync(DiscordClient client, MessageDeletedEventArgs e)
        {
            var msg = e.Message;
            string? content = msg?.Content;
            ulong? authorId = msg?.Author?.Id;
            var mentions = msg?.MentionedUsers?.Select(x => x.Id).ToList();

            //fallback to cache
            if (string.IsNullOrWhiteSpace(content) || authorId is null || mentions is null)
            {
                if (cache.TryGet(e.Message.Id, out var cached))
                {
                    content ??= cached?.Content;
                    authorId ??= cached?.AuthorId;
                    mentions ??= [.. cached!.MentionedUserIds];
                }
            }

            // Still missing data? Abort
            if (string.IsNullOrWhiteSpace(content) || authorId is null || mentions is null) return;
            if (mentions.Count == 0) return;

            var author = await client.GetUserAsync(authorId.Value);
            if (author.IsBot) return;

            // Notify mentioned users
            foreach (var mentionedId in mentions)
            {
                await rateLimitHelper.WaitForRateLimitAsync(mentionedId);
                var user = await client.GetUserAsync(mentionedId);
                await user.SendMessageAsync(
                    $"You were ghost pinged by **{author.Username}** in **#{e.Channel.Name}**.\nGuild: **{e.Guild.Name}**\n**Deleted message:** {content}");

                //lets log the deleted message to the support server for safe keeping
                ulong logChannelId = 888659027607814214;
                var channel = await client.GetChannelAsync(logChannelId);

                await rateLimitHelper.WaitForRateLimitAsync(mentionedId);
                await channel.SendMessageAsync($"Ghost Ping Detected | User: {author.Username} Deleted a message for {user.Username} with content: {content}\nGuild: **{e.Guild.Name}**\nChannel: **#{e.Channel.Name}**");
                
            }

            // Remove from cache
            cache.Remove(e.Message.Id);
        }
        #endregion
    }
}
