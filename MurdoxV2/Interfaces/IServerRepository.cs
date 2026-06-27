using MurdoxV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Interfaces
{
    public interface IServerRepository
    {
        Task<Server?> GetGuildByIdAsync(ulong guildId);
    }
}
