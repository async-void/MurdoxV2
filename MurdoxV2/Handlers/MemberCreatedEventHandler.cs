using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MurdoxV2.Services.Welcomer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Handlers
{
    public sealed class MemberCreatedEventHandler(IWelcomer welcomer) : IEventHandler<GuildMemberAddedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, GuildMemberAddedEventArgs eventArgs)
        {
            if (eventArgs.Member.IsBot) return;
            var defaultChannel = eventArgs.Guild.GetDefaultChannel() ??
                eventArgs.Guild.Channels
                     .Values
                     .Where(c => c.Type == DiscordChannelType.Text)
                     .OrderBy(c => c.Position)
                     .FirstOrDefault();

            if (defaultChannel is null) return; //short circuit if there are no text channels in the users guild.

            var msg = await welcomer.GetMemberWelcomeAsync(eventArgs.Member);
            var sanitizedMsg = msg.Message.Replace("{username}", $"<@{eventArgs.Member.Id}>");
            var dmMsg = $"{sanitizedMsg} please abite by our Guild rules!";
            DiscordComponent[] comps = 
            {
                    new DiscordTextDisplayComponent($"{msg.Emoji} {sanitizedMsg}")
            };
            
            var container = new DiscordContainerComponent(comps, false, DiscordColor.Turquoise);
            var msgBuilder = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);

            DiscordComponent[] dmComps =
            {
                    new DiscordTextDisplayComponent($"{msg.Emoji} {dmMsg}")
            };

            var dmContainer = new DiscordContainerComponent(dmComps, false, DiscordColor.Turquoise);
            var dmMsgBuilder = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);

            await eventArgs.Member.SendMessageAsync(dmMsgBuilder);
            await sender.SendMessageAsync(defaultChannel, msgBuilder);
        }
    }
}
