using DSharpPlus;
using DSharpPlus.Entities;
using MurdoxV2.Enums;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using MurdoxV2.Services.Tickets;

namespace MurdoxV2.Coordinators
{
    public sealed class TicketCoordinator(ITicket service)
    {
        private readonly ITicket _service = service;

        public async Task<Ticket> CreateTicketAsync(DiscordClient client, DiscordChannel channel, DiscordUser user, TicketType type, string details)
        {
            // 1. Create ticket object
            var ticket = _service.CreateTicket(user.Id, channel.Guild.Id, type, details);
            if (ticket is null)
                return null;

            // 2. Create parent message
            var parentMessage = await channel.SendMessageAsync($"Ticket: {ticket.TicketId} has been created");

            // 3. Create thread
            var thread = await channel.CreateThreadAsync(
                parentMessage,
                $"ticket-{ticket.TicketId}",
                DiscordAutoArchiveDuration.ThreeDays
            );

            var callingMember = await channel.Guild.GetMemberAsync(user.Id);
            await thread.AddThreadMemberAsync(callingMember);
            ticket.ThreadId = thread.Id;

            return ticket;
        }

        public async Task<bool> CloseTicketAsync(ulong ticketId)
        {
            await _service.CloseTicket(ticketId);
            return true;
        }

        public async Task<List<Ticket>> GetOpenTicketsForUserAsync(ulong guildId, ulong userId)
        {
            return await _service.GetOpenTicketsForUser(guildId, userId);
        }
    }
}
