using MurdoxV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Interfaces
{
    public interface ITagRepository
    {
        Task<Tag?> GetAsync(ulong guildId, ulong tagId, CancellationToken ct = default);
        Task<Tag?> GetByNameAsync(ulong guildId, string name, CancellationToken ct = default);
        Task<IReadOnlyList<Tag>> GetAllAsync(ulong guildId, CancellationToken ct = default);
        Task<bool> AddAsync(Tag tag, CancellationToken ct = default);
        Task UpdateAsync(Tag tag, CancellationToken ct = default);
        Task<bool> DeleteAsync(Tag tag, CancellationToken ct = default);
        Task<bool> ExistsAsync(ulong guildId, string name, CancellationToken ct = default);
    }
}
