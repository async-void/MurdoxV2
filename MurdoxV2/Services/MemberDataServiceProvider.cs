using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Enums;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;

namespace MurdoxV2.Services
{
    public class MemberDataServiceProvider(IDbContextFactory<AppDbContext> dbFactory, DiscordClient client) : IMemberData
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
        private readonly DiscordClient _client = client;

        #region GET MEMBER
        public async Task<Result<ServerMember, SystemError<MemberDataServiceProvider>>> GetMemberAsync(ulong guildId, ulong discordId)
        {
            var db = _dbFactory.CreateDbContext();
            var mem = await db.Members
                .Include(m => m.Bank)
                .Include(b => b.Reminders)
                .Include(m => m.Tickets)
                .AsSplitQuery()
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

        #region GET ALL MEMBERS FOR GUILD

      

        #endregion

        #region GET OR CREATE MEMBER

        public async Task<ServerMember> GetOrCreateMemberAsync(ulong memberId, ulong guildId)
        {
            var db = _dbFactory.CreateDbContext();
            var mem = await db.Members
                .Include(m => m.Bank)
                .Include(b => b.Reminders)
                .Include(m => m.Tickets)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m => m.GuildId == guildId && m.DiscordId == memberId);
            if (mem is not null)
                return mem;

            var dGuild = await _client.GetGuildAsync(guildId);
            var dMem = await dGuild.GetMemberAsync(memberId);

            var bank = new Bank
            {
                Balance = 100,
                Deposit_Amount = 100,
                Withdraw_Amount = 0,
                Deposit_Timestamp = DateTime.UtcNow,
            };

            var newUser = new ServerMember
            {
                DiscordId = memberId,
                GuildId = guildId,
                Discriminator = dMem.Discriminator,
                AvatarUrl = dMem.AvatarUrl,
                XP = 100,
                MessageCount = 1,
                JoinedAt = dMem.JoinedAt,
                CreatedAt = DateTime.UtcNow,
                IsBanned = false,
                IsBot = false,
                IsMuted = false,
                Bank = bank
            };
            return newUser;
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
                {
                    db.Entry(member).CurrentValues.SetValues(mem);
                    await db.SaveChangesAsync();
                    return true;
                }

                return false;
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

        #region GET MEMBER XP
        public async Task<int> GetMemberXPAsync(ulong memberId, ulong guildId)
        {
            var db = _dbFactory.CreateDbContext();
            var member = await db.Members.FirstOrDefaultAsync(x => x.DiscordId == memberId && x.GuildId == guildId);
            if (member is not null)
            {
                var xp = member.XP;
                return xp;
            }
            return -1;
        }
        #endregion  
    }
}
