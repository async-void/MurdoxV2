using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.SlashCommands.Moderation
{
    public class ModerationCommands()
    {
        [Command("mod")]
        [Description("handle moderation commands")]
        [RequirePermissions(DiscordPermission.ManageGuild)]
        public async Task ModCommand(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();
            
            var btns = new DiscordComponent[]
            {
                new DiscordButtonComponent(DiscordButtonStyle.Primary, "purgeBtn", "Purge"),
                new DiscordButtonComponent(DiscordButtonStyle.Danger, "banBtn", "Ban Member"),
                new DiscordButtonComponent(DiscordButtonStyle.Secondary, "warnBtn", "Warn Member"),
            };

            var components = new DiscordComponent[]
            {
                new DiscordTextDisplayComponent("**Moderation Commands**"),
                new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                new DiscordTextDisplayComponent("Choose a mod command below"),
                new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Small),
                new DiscordSectionComponent("Purge Messages", new DiscordButtonComponent(DiscordButtonStyle.Primary, "purgeBtn", "Purge")),
                new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Small),
                new DiscordSectionComponent("Warn Member", new DiscordButtonComponent(DiscordButtonStyle.Primary, "warnBtn", "Warn")),
                new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Small),
                new DiscordSectionComponent("Ban Member", new DiscordButtonComponent(DiscordButtonStyle.Danger, "banBtn", "Ban")),
            };

            var container = new DiscordContainerComponent(components, false, DiscordColor.Gray);

            var msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);
            await ctx.EditResponseAsync(msg);

        }
    }
}
