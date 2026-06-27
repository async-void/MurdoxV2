using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Repositories
{
    public class ServerRepository(IDbContextFactory<AppDbContext> dbFactory) : IServerRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

        public async Task<Server?> GetGuildByIdAsync(ulong guildId)
        {
            await using var dbContext = await _dbFactory.CreateDbContextAsync();

            return await dbContext.Guilds
                .Include(s => s.Members)
                .FirstOrDefaultAsync(s => s.GuildId == guildId);
        }

    }
}
