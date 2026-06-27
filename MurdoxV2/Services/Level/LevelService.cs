using DSharpPlus.Entities;
using MurdoxV2.Interfaces;
using MurdoxV2.Services.Builders.Level;
using System.Collections.Concurrent;

namespace MurdoxV2.Services.Level
{
    public sealed class LevelService(IMemberData memDataService): ILevel
    {
        private readonly IMemberData _memDataService = memDataService;

        private readonly ConcurrentDictionary<ulong, DateTime> _cooldowns = new();
        private readonly TimeSpan _cooldown = TimeSpan.FromSeconds(60);
        private readonly Random _random = new();

        public static int XpForLevel(int level) => 5 * (level * level) + 50 * level + 100;

        public async Task HandleMessageAsync(DiscordMember member)
        {
            if (_cooldowns.TryGetValue(member.Id, out var lastTime) && DateTime.UtcNow - lastTime < _cooldown)
                return;

            _cooldowns[member.Id] = DateTime.UtcNow;

            int xpGain = _random.Next(1, 5);
            var user = await _memDataService.GetOrCreateMemberAsync(member.Id, member.Guild.Id);
            var oldXp = user.XP;
            user.XP += xpGain;
            user.MessageCount++;

            int oldLevel = GetLevel(oldXp);
            int newLevel = GetLevel(user.XP);

            await _memDataService.SaveMemberAsync(user);

            if (newLevel > oldLevel)
                await OnLevelUpAsync(member, newLevel);
        }

        #region ONLEVELUP

        public async Task OnLevelUpAsync(DiscordMember member, int newLevel)
        {
            var channel = member.Guild.GetDefaultChannel();
            if (channel is null)
            {
                var channels = await member.Guild.GetChannelsAsync();
                channel = channels.FirstOrDefault(x => x.Type == DiscordChannelType.Text);
                await channel.SendMessageAsync($"🎉 {member.Mention} leveled up to **Level {newLevel}**!");
            }

            await channel.SendMessageAsync($"🎉 {member.Mention} leveled up to **Level {newLevel}**!");
        }

        #endregion

        #region GET LEVEL

        private static int GetLevel(long totalXp)
        {
            int level = 0;
            long xpNeeded = XpForLevel(level);

            while (totalXp >= xpNeeded)
            {
                totalXp -= xpNeeded; ;
                level++;
                xpNeeded = XpForLevel(level);
            }
            return level;
        }

        #endregion

        #region GET PROGRESS

        public async Task<(int level, int currentXp, int neededXp)> GetProgressAsync(long totalXp)
        {
            int level = 0;
            long xpNeeded = XpForLevel(level);
             
            while(totalXp >= xpNeeded)
            {
                totalXp -= xpNeeded;
                level++;
                xpNeeded = XpForLevel(level);
            }

            return (level, (int)totalXp, (int)xpNeeded);
        }
        #endregion
    }
}
