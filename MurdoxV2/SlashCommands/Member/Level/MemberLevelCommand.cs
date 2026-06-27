using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using MurdoxV2.Services.Builders.Level;
using System.ComponentModel;

namespace MurdoxV2.SlashCommands.Member.Level
{
    public class MemberLevelCommand(XpLevelBuilderService xpLevelService)
    {
        [Command("level")]
        [Description("get member xp level")]
        public async ValueTask MemberLevel(SlashCommandContext ctx, [Parameter("user")] DiscordUser user)
        {
            await ctx.DeferResponseAsync();
            if (ctx.Guild is null)
                return;
            var member = await ctx.Guild.GetMemberAsync(user.Id);
            if (member is null)
                return;
            var stream = await xpLevelService.BuildLevelBitmapAsync(member);

            var builder = new DiscordWebhookBuilder() // or DiscordInteractionResponseBuilder
                .EnableV2Components()
                .AddFile("level.png", stream);
 
            var mediaItem = new DiscordMediaGalleryItem("attachment://level.png");
            var mediaGallery = new DiscordMediaGalleryComponent(mediaItem);

            var container = new DiscordContainerComponent(
                [
                    new DiscordTextDisplayComponent($"Level for {member.Mention}!"),
                    mediaGallery
                ],
                color: new DiscordColor("#7160e8")
            );

            builder.AddContainerComponent(container);

            await ctx.EditResponseAsync(builder);

            stream.Dispose();
        }
    }
}
