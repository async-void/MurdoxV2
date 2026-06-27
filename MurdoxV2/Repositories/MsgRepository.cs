using MurdoxV2.Models;
using MurdoxV2.Services.MessageCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Repositories
{
    public sealed class MsgRepository(DiscordMessageCacheService msgCache)
    {
        private readonly DiscordMessageCacheService _msgCache = msgCache;

        #region GET BY MEMBER ID

        //public async Task<List<CachedDiscordMessage>> GetByMemberId(ulong memberId)
        //{
        //    _msgCache.TryGet()
        //}

        #endregion
    }
}
