using MurdoxV2.Enums;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;

namespace MurdoxV2.Cache.TicketCache
{
    public sealed class TicketCache
    {
        private readonly object _lock = new();
        private readonly Dictionary<ulong, Ticket> _byId = [];
        private readonly Dictionary<(ulong GuildId, ulong UserId), Ticket> _openByGuildUser = [];

        public bool TryGetOpenTicket(ulong guildId, ulong userId, out Ticket ticket)
            => _openByGuildUser.TryGetValue((guildId, userId), out ticket!);

        public bool TryAdd(Ticket ticket)
        {
            lock (_lock)
            {
                if (_byId.ContainsKey(ticket.TicketId))
                    return false;

                var key = (ticket.GuildId, ticket.UserId);

                if (ticket.Status == TicketStatus.Open &&
                    _openByGuildUser.ContainsKey(key))
                    return false;

                _byId[ticket.TicketId] = ticket;

                if (ticket.Status == TicketStatus.Open)
                    _openByGuildUser[key] = ticket;

                return true;
            }
        }

        public Ticket? MarkClosed(ulong ticketId)
        {
            if (!_byId.TryGetValue(ticketId, out var ticket))
                return null;

            ticket.Status = TicketStatus.Closed;
            ticket.ClosedAt = DateTimeOffset.UtcNow;
            return ticket;
        }

    }
}