using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using System.ComponentModel;

namespace MurdoxV2.SlashCommands.Economy.DiceRoller;

public class DiceRollerCommands
{
    [Command("roll")]
    [Description("roll a set of dice")]
    public async ValueTask RollDice(SlashCommandContext ctx)
    {
        await ctx.DeferResponseAsync();
        var member = ctx.Member;
        var channel = ctx.Channel;
        var guild = ctx.Guild;

        DiscordComponent[] comps =
        [
            new DiscordTextDisplayComponent($"# Dice Roller"),
            new DiscordSeparatorComponent(true),
            new DiscordTextDisplayComponent($"{member!.DisplayName} rolled []"),
            new DiscordMediaGalleryComponent(new DiscordMediaGalleryItem("https://i.imgur.com/ZH33780.png"))
        ];
        var container = new DiscordContainerComponent(comps, false, DiscordColor.Blurple);

        var msg = new DiscordMessageBuilder()
            .EnableV2Components()
            .AddContainerComponent(container);

        await ctx.RespondAsync(msg);
    }
    
}
