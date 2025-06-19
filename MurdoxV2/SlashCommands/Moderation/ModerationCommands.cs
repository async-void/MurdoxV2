using DSharpPlus.Commands;
using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.SlashCommands.Moderation
{
    public class ModerationCommands(IDbContextFactory<AppDbContext> dbFactory)
    {
        [Command("add_member")]
        [Description("Adds a member to the Server.")]
        public async Task AddMember(CommandContext ctx, [Parameter("username")] string username)
        {
            var db = dbFactory.CreateDbContext();
            var member = db.Members.FirstOrDefault(m => m.GlobalUsername == username && m.ServerId == ctx.Guild.Id.ToString());
        }
    }
}
