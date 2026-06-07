using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Enums;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;

namespace MurdoxV2.Services
{
    public class MemberDataServiceProvider(IDbContextFactory<AppDbContext> dbFactory) : IMemberData
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

        #region GET MEMBER
        public async Task<Result<ServerMember, SystemError<MemberDataServiceProvider>>> GetMemberAsync(ulong guildId, ulong discordId)
        {
            var db = _dbFactory.CreateDbContext();
            var mem = await db.Members
                .Include(m => m.Bank)
                .Include(b => b.Reminders)
                .Include(m => m.Tickets)
                .FirstOrDefaultAsync(m => m.GuildId == guildId && m.DiscordId == discordId);

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
        #endregion

        #region SAVE MEMBER
        public async Task<Result<bool, SystemError<MemberDataServiceProvider>>> SaveMemberAsync(ServerMember mem)
        {
            try
            {
                var db = _dbFactory.CreateDbContext();
                var member = await db.Members
                    .FirstOrDefaultAsync(m => m.GuildId == mem.GuildId && m.DiscordId == mem.DiscordId);

                if (member is not null)
                    return true;

                await db.AddAsync(mem);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return Result<bool, SystemError<MemberDataServiceProvider>>.Err(new SystemError<MemberDataServiceProvider>
                {
                    ErrorMessage = e.Message,
                    ErrorType = ErrorType.WARNING, 
                    CreatedAt= DateTimeOffset.UtcNow,
                    CreatedBy= this
                });
            }
            
        }
        #endregion

        #region UPDATE MEMBER
        public Task<Result<bool, SystemError<MemberDataServiceProvider>>> UpdateMemberAsync(ServerMember mem)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
