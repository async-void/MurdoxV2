using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MurdoxV2.Cache.TicketCache;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Enums;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using MurdoxV2.Utilities.SnowflakeIds.TicketId;

namespace MurdoxV2.Services.Tickets
{
    public class TicketDataServiceProvider(DiscordClient client, TicketCache store, IDbContextFactory<AppDbContext> dbFactory) : ITicket
    {
        private readonly TicketCache _store = store;
        private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
        private readonly DiscordClient _client = client;
        public Ticket CreateTicket(ulong userId, ulong guildId, TicketType type, string details)
        {
            var ticket = new Ticket
            {
                TicketId = TicketSnowflakeGenerator.NextId(),
                UserId = userId,
                GuildId = guildId,
                Type = type,
                Content = details,
                Status = TicketStatus.Open,
                CreatedAt = DateTimeOffset.UtcNow
            };

            if (!_store.TryAdd(ticket))
                return null;

            using var db = _dbFactory.CreateDbContext();
            db.Tickets.Add(ticket);
            db.SaveChanges();

            return ticket;
        }

        public async Task<bool> CloseTicket(ulong ticketId)
        {
            // 1. Update in-memory cache
            _store.MarkClosed(ticketId);

            // 2. Update database
            using var db = _dbFactory.CreateDbContext();

            var dbTicket = db.Tickets.FirstOrDefault(t => t.TicketId == ticketId);
            if (dbTicket is null)
                return false;

            var guild = await _client.GetGuildAsync(dbTicket.GuildId);
            var thread = await guild.GetChannelAsync(dbTicket.ThreadId);

            if (thread is not null) await thread.DeleteAsync();
            dbTicket.Status = TicketStatus.Closed;
            dbTicket.ClosedAt = DateTimeOffset.UtcNow;

            db.SaveChanges();
            return true;
        }

        public async Task<List<Ticket>> GetOpenTicketsForUser(ulong guildId, ulong userId)
        {
            if (_store.TryGetOpenTicket(guildId, userId, out var ticket))
                return [ticket];
            using var db = _dbFactory.CreateDbContext();
            return await db.Tickets
                .Where(t => t.GuildId == guildId && t.UserId == userId && t.Status == TicketStatus.Open)
                .ToListAsync();

        }
    }
}
