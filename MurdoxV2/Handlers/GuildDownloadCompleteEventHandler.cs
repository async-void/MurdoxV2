using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Handlers
{
    internal class GuildDownloadCompleteEventHandler(IDbContextFactory<AppDbContext> dbFactory) : IEventHandler<GuildDownloadCompletedEventArgs>
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
        public async Task HandleEventAsync(DiscordClient sender, GuildDownloadCompletedEventArgs eventArgs)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            //--------------------------------------//
            // ADD ALL GUILDS TO DATABASE ON STARTUP
            //--------------------------------------//
            var guilds = eventArgs.Guilds.ToList();
            foreach (var guild in guilds)
            {
                var exists = await db.Guilds.AnyAsync(x => x.GuildId == guild.Value.Id);
                if (exists)
                    continue;

                var members = guild.Value.Members.ToList();
                var memList = new List<ServerMember>();
                var ownerId = guild.Value.OwnerId;
                var owner = await guild.Value.GetMemberAsync(ownerId);
                var notificationChannel = guild.Value.Channels.FirstOrDefault(x => x.Value.Type == DiscordChannelType.Text);
                foreach (var member in members)
                {
                    memList.Add(new ServerMember
                    {
                        GuildId = guild.Value.Id,
                        DiscordId = member.Value.Id,
                        GlobalUsername = member.Value.Username,
                        Nickname = member.Value.Nickname,
                        AvatarUrl = member.Value.AvatarUrl,
                        Discriminator = member.Value.Discriminator,
                        JoinedAt = member.Value.JoinedAt.ToUniversalTime(),
                        Bank = new Bank()
                    });
                }

                var guildRecord = new Server
                {
                    GuildId = guild.Value.Id,
                    GuildName = guild.Value.Name,
                    OwnerUsername = owner.Username,
                    Members = memList,
                    CreatedAt = guild.Value.CreationTimestamp.ToUniversalTime(),
                    OwnerId = owner.Id,
                    NotificationChannelId = notificationChannel.Value.Id,

                };

                await db.Guilds.AddAsync(guildRecord);
                await db.SaveChangesAsync();

            }
        }
    }
}
