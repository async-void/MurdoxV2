

using DSharpPlus;
using DSharpPlus.EventArgs;
using MurdoxV2.Services.MessageCache;

namespace MurdoxV2.Handlers
{
    public class MessageDeletedEventHandler(GhostPingService ghostPingService) : IEventHandler<MessageDeletedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, MessageDeletedEventArgs eventArgs)
        {
            var msg = eventArgs.Message;

            if (msg.MentionedUsers.Any())
            {
                await ghostPingService.HandleSingleDeleteAsync(sender, eventArgs);
                return;
            }

            //https://i.imgur.com/C575c6Q.png - ghost ping image
            string result = msg.Author is not null ? $"<@{msg.Author.Id}>": "UNKNOWN";
            await sender.SendMessageAsync(eventArgs.Channel, $"<#{result}> I caught your message deletion and logged it for the mods to inspect.");

        }
    }
}
    