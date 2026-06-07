using MurdoxV2.Enums;
using MurdoxV2.Models;
using MurdoxV2.Services.Tickets;

namespace MurdoxV2.Interfaces
{
    public interface ITicket
    {
        Ticket CreateTicket(ulong userId, ulong guildId, TicketType type, string details);
        Task<bool> CloseTicket(ulong ticketId);
        Task<List<Ticket>> GetOpenTicketsForUser(ulong guildId, ulong userId);
    }
}
