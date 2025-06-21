using DSharpPlus.Commands;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.SlashCommands.Utility
{
    public class UtilityCommand
    {
        [Command("utility")]
        [Description("a set of utility commands")]
        public async Task UtilityCommandAsync(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();
            var components = new DiscordComponent[]
            {
                new DiscordTextDisplayComponent("# Utility Commands"),
                new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                new DiscordTextDisplayComponent("Choose a utility command below"),
                new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Small),
                new DiscordSectionComponent("Murdox's Uptime", new DiscordButtonComponent(DiscordButtonStyle.Primary, "uptimeBtn", "Uptime")),
                new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Small),
                new DiscordSectionComponent("Ping Information", new DiscordButtonComponent(DiscordButtonStyle.Primary, "pingBtn", "Ping")),
                new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                new DiscordSectionComponent(new DiscordTextDisplayComponent("Donatations help to keep Murdox online!"),
                                        new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate")),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"-# Murdox ©️ {DateTime.UtcNow:ddd, MM-dd-yyyy hh:mm tt}")
            };
            var container = new DiscordContainerComponent(components, false, DiscordColor.Gray);
            var msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);
            await ctx.EditResponseAsync(msg);
        }
    }
}
