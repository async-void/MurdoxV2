using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MurdoxV2.Factories;
using MurdoxV2.Models;
using MurdoxV2.Services.Welcomer;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Handlers
{
    public class GuildAddedEventHandler(AppDbContextFactory dbFactory, ILogger<GuildAddedEventHandler> logger, IWelcomer welcomerService) : IEventHandler<GuildCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, GuildCreatedEventArgs eventArgs)
        {
            var db = dbFactory.CreateDbContext();
            var guild = await db.Guilds
                .AsNoTracking()
                .Where(g => g.GuildId == eventArgs.Guild.Id)
                .FirstOrDefaultAsync();
            if (guild is not null) { }
            else
            {
                var _guild = new Server()
                {
                    GuildId = eventArgs.Guild.Id,
                    GuildName = eventArgs.Guild.Name,
                    OwnerId = eventArgs.Guild.OwnerId,
                    OwnerUsername = eventArgs.Guild.GetMemberAsync(eventArgs.Guild.OwnerId).ToString() ?? "Unknown",
                    NotificationChannelId = eventArgs.Guild.GetDefaultChannel()!.Id,
                    CreatedAt = DateTimeOffset.UtcNow,
                    EnableFacts = false,
                    Members = [.. eventArgs.Guild.Members.Select(m => new ServerMember
                                {
                                    DiscordId = m.Value.Id,
                                    GuildId = eventArgs.Guild.Id,
                                    GlobalUsername = m.Value.Username,
                                    Nickname = m.Value.Nickname ?? "No Nickname",
                                    AvatarUrl = m.Value.AvatarUrl,
                                    Discriminator = m.Value.Discriminator,
                                    IsBot = m.Value.IsBot,
                                    JoinedAt = m.Value.JoinedAt,
                                    UserStatus = m.Value.Presence?.Status.ToString(),
                                    XP = 0,
                                    MessageCount = 0,
                                    Bank = new Bank()
                                    {
                                        Balance = 100,
                                        Deposit_Amount = 100,
                                        Withdraw_Amount = 0,
                                        Deposit_Timestamp = DateTimeOffset.UtcNow,
                                        Withdraw_Timestamp = null,
                                    }
                                })]
                };
                await db.Guilds.AddAsync(_guild);
                await db.SaveChangesAsync();
                await welcomerService.GetGuildWelcomeAsync(eventArgs.Guild);
            }
            logger.LogInformation("Guild added: {name} ({id})", eventArgs.Guild.Name, eventArgs.Guild.Id);
        }
    }
}
