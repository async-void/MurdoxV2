using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using HtmlAgilityPack;
using MurdoxV2.Coordinators;
using MurdoxV2.Enums;
using System.ComponentModel;

namespace MurdoxV2.SlashCommands.Member.Ticket
{
    public class TicketCommand(TicketCoordinator coordinator)
    {
        [Command("ticket")]
        [Description("Create a new support ticket.")]
        public async ValueTask CreateTicketAsync(CommandContext ctx, [Parameter("type")] [Description("ticket type")] string type, [Parameter("issue")] [Description("a brief description of the issue")] string issue)
        {
            await ctx.DeferResponseAsync();
            var convertedType = type.ToLower() switch
            {
                "support" => TicketType.Support,
                "bug" => TicketType.Bug,
                "other" => TicketType.Other,
                _ => TicketType.Other,
            };
            var ticket = await coordinator.CreateTicketAsync( ctx.Client, ctx.Channel, ctx.User, convertedType, issue);

            if (ticket is null)
            {
                await ctx.RespondAsync("You already have an open ticket.");
                return;
            }

            try
            {
                if (ctx.Guild is null)
                {
                    await ctx.RespondAsync("`this command must be used in a Guild`");
                    return;
                }
                var callingMember = await ctx.Guild.GetMemberAsync(ctx.User.Id);
                await callingMember.SendMessageAsync(new DiscordMessageBuilder()
                    .WithContent($"Your ticket has been created: <#{ticket.ThreadId}>"));
            }
            catch (UnauthorizedException)
            {
                // User has DMs disabled
                await ctx.RespondAsync("`Your ticket was created, but I couldn't DM you the link because your DMs are closed.`");
                return;
            }

            await ctx.RespondAsync("`Ticket created successfully!`");
        }

        [Command("close-ticket")]
        [Description("Close a support ticket.")]
        public async ValueTask CloseTicket(SlashCommandContext ctx, [Parameter("id")] ulong ticketId)
        {
            await ctx.DeferResponseAsync();
           
            var success = await coordinator.CloseTicketAsync(ticketId);
            if (!success)
            {
                await ctx.RespondAsync("`Failed to close ticket. It may have already been closed or does not exist.`");
                return;
            }
            await ctx.RespondAsync("`Ticket closed successfully!`");
        }
    }
}
