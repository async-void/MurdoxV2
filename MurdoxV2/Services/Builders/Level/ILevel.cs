using DSharpPlus.Entities;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Services.Builders.Level
{
    public interface ILevel
    {
        Task HandleMessageAsync(DiscordMember member);
        Task<(int level, int currentXp, int neededXp)> GetProgressAsync(long totalXp);
    }
}
