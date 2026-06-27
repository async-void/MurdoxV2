

using DSharpPlus;
using DSharpPlus.EventArgs;
using MurdoxV2.Services.MessageCache;

namespace MurdoxV2.Handlers
{
    public class MessageDeletedEventHandler(GhostPingService ghostPingService, DiscordMessageCacheService cache) : IEventHandler<MessageDeletedEventArgs>
    {
        private readonly DiscordMessageCacheService _cache = cache;
        public async Task HandleEventAsync(DiscordClient sender, MessageDeletedEventArgs eventArgs)
        {
            var msg = eventArgs.Message;

            // Try Discord's data first
            var author = msg.Author;
            var mentions = msg.MentionedUsers;

            // Fallback to cache if needed
            if (author is null || mentions is null || mentions.Count == 0)
            {
                if (_cache.TryGet(msg.Id, out var cached))
                {
                    author = await sender.GetUserAsync(cached.AuthorId);
                    mentions = [.. cached.MentionedUserIds.Select(id => sender.GetUserAsync(id).Result)];
                }
            }

            // If still no author, we cannot process
            if (author is null)
                return;

            // Ignore bot deletions
            if (author.IsBot)
                return;

            if (msg.MentionedUsers.Any())
            {
                await ghostPingService.HandleSingleDeleteAsync(sender, eventArgs);
                return;
            }

            //https://i.imgur.com/C575c6Q.png - ghost ping image
            string result = msg.Author is not null ? $"<@{msg.Author.Id}>": "UNKNOWN";
            await sender.SendMessageAsync(eventArgs.Channel, $"{result} I caught your message deletion and logged it for the mods to inspect.");

        }
    }
}
    