using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MurdoxV2.Services.Tags
{
    public class TagRepositoryProviderService(IDbContextFactory<AppDbContext> dbFactory) : ITagRepository
    {
        private readonly AppDbContext db = dbFactory.CreateDbContext();
        public async Task<bool> AddAsync(Tag tag, CancellationToken ct = default)
        {
            var exists = await ExistsAsync(tag.GuildId, tag.Name, ct);

            if (exists)
                return false;

            db.Tags.Add(tag);
            await db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(Tag tag, CancellationToken ct = default)
        {
            var exists = await ExistsAsync(tag.GuildId, tag.Name, ct);

            if (!exists)
                return false;

            db.Tags.Remove(tag);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(ulong guildId, string name, CancellationToken ct = default)
        {
            bool exists = await db.Tags
                .AnyAsync(t => t.GuildId == guildId && t.Name == name, ct);

            if (exists)
                return true;
            return false;
        }

        public async Task<IReadOnlyList<Tag>> GetAllAsync(ulong guildId, CancellationToken ct = default)
        {
            return (await db.Tags
                     .Where(t => t.GuildId == guildId)
                     .ToListAsync(ct))
                     .AsReadOnly();
        }

        public async Task<Tag?> GetAsync(ulong guildId, ulong tagId, CancellationToken ct = default)
        {
            return await db.Tags
                    .FirstOrDefaultAsync(t => t.GuildId == guildId && t.TagId == tagId, ct);

        }

        public async Task<Tag?> GetByNameAsync(ulong guildId, string name, CancellationToken ct = default)
        {
            return await db.Tags.FirstOrDefaultAsync(t => t.Name == name && t.GuildId == guildId, cancellationToken: ct);
        }

        public Task UpdateAsync(Tag tag, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
