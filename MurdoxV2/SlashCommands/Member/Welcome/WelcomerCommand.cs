using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using MurdoxV2.Services.Welcomer;
using System.ComponentModel;

namespace MurdoxV2.SlashCommands.Member.Welcome
{
    public class WelcomerCommand(IWelcomer welcomerService)
    {
        [Command("welcome")]
        [Description("welcome a new member to the Guild")]
        public async ValueTask WelcomeMember(SlashCommandContext ctx, [Parameter("member")] DiscordUser user)
        {
            try
            {
                await ctx.DeferResponseAsync();
                var guild = ctx.Guild ?? throw new InvalidOperationException("Guild is null.");
                var member = await guild.GetMemberAsync(user.Id);
                var welcomeMsg = await welcomerService.GetMemberWelcomeAsync(member);

                if (welcomeMsg is not null)
                {
                    var emoji = welcomeMsg.Emoji;
                    var msg = welcomeMsg?.Message.Replace("{username}", $"<@{member.Id}>").Replace("the server", $"{ctx.Guild.Name}");
                    await ctx.RespondAsync($"{emoji} {msg}");
                }
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync($"Something went horribly wrong - Error: {ex.Message}");
            }
            
        }
    }
}
