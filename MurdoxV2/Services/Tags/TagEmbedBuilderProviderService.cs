using DSharpPlus.Entities;
using MurdoxV2.Extensions;
using MurdoxV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Services.Tags
{
    public sealed class TagEmbedBuilderProviderService
    {
        public DiscordMessageBuilder BuildTagEmbed(Tag tag, bool isSpoiled, DiscordColor color)
        {
            var timestamp = DateTimeOffset.UtcNow.ToTimestamp();
            var links = "======================================\n";
            if (tag.TagLinks is not null && tag.TagLinks.Count > 0)
            {
                links += string.Join("\n", tag.TagLinks.Select(x => $"- {x}"));
            }
            DiscordComponent[] comps =
            [
                new DiscordTextDisplayComponent($"## Tag {tag.Name}"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"- Author: {tag.Author}"),
                new DiscordTextDisplayComponent($"- Created At: {tag.CreatedAt}"),
                new DiscordTextDisplayComponent($"- Last Modified: {tag.LastModifiedAt}"),
                new DiscordTextDisplayComponent($"- Modified By: {tag.LastModifiedAt}"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"{tag.Content}"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"{links}"),
                new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                new DiscordSectionComponent(new DiscordTextDisplayComponent($"Murdox ©️ {timestamp}"),
                    new DiscordButtonComponent(DiscordButtonStyle.Primary, "donate", "Donate"))
                //TODO: finish the comps build.
            ];
            var container = new DiscordContainerComponent(comps, isSpoiled, color);
            var msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);
            return msg; 
        }
    }
}
