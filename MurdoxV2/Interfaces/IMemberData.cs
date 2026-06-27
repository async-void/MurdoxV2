using MurdoxV2.Models;
using MurdoxV2.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Interfaces
{
    public interface IMemberData
    {
        Task<Result<ServerMember, SystemError<MemberDataServiceProvider>>> GetMemberAsync(ulong guildId, ulong discordId);
        Task<ServerMember> GetOrCreateMemberAsync(ulong memberId, ulong guildId);
        Task<Result<bool, SystemError<MemberDataServiceProvider>>> UpdateMemberAsync(ServerMember mem);
        Task<Result<bool, SystemError<MemberDataServiceProvider>>> SaveMemberAsync(ServerMember mem);
        Task<int> GetMemberXPAsync(ulong memberId, ulong guildId);
    }
}
