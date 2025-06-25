using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Enums;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Services
{
    public class MemberDataServiceProvider(IDbContextFactory<AppDbContext> dbFactory) : IMemberData
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
        public async Task<Result<ServerMember, SystemError<MemberDataServiceProvider>>> GetMemberAsync(string guildId, string discordId)
        {
            var db = _dbFactory.CreateDbContext();
            var mem = await db.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.GuildId.Equals(guildId) && m.DiscordId.Equals(discordId));

            return mem is not null
                   ? Result<ServerMember, SystemError<MemberDataServiceProvider>>.Ok(mem)
                   : Result<ServerMember, SystemError<MemberDataServiceProvider>>.Err(new SystemError<MemberDataServiceProvider>
                   {
                       ErrorMessage = $"Member with Discord ID {discordId} not found in Guild {guildId}.",
                       ErrorType = ErrorType.NOTFOUND,
                       CreatedAt = DateTime.UtcNow,
                       CreatedBy = this
                   });

        }

        public Task<Result<bool, SystemError<MemberDataServiceProvider>>> SaveMemberAsync(ServerMember mem)
        {
            throw new NotImplementedException();
        }

        public Task<Result<bool, SystemError<MemberDataServiceProvider>>> UpdateMemberAsync(ServerMember mem)
        {
            throw new NotImplementedException();
        }

        public static async Task<Result<bool, SystemError<MemberDataServiceProvider>>> SaveMemberDataOnCloseAsync(Dictionary<string, int> userData)
        {
            throw new NotImplementedException();
        }
    }
}
