using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Features.ScamDetection
{
    public sealed class ScamImageContext(DiscordGuild guild, DiscordChannel channel, DiscordMessage message, DiscordAttachment attachment, DiscordUser user)
    {
        public DiscordGuild Guild { get; } = guild;
        public DiscordChannel Channel { get; } = channel;
        public DiscordMessage Message { get; } = message;
        public DiscordAttachment Attachment { get; } = attachment;
        public DiscordUser User { get; } = user;
    }
}
