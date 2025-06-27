using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Factories;
using MurdoxV2.Models;
using MurdoxV2.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Utilities.OnAppClosing
{
    public class CleanUp(IDbContextFactory<AppDbContext> dbFactory)
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

        public async Task<Result<bool, SystemError<CleanUp>>> SaveMemberDataOnCloseAsync(Dictionary<ServerMember, int> userData)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();
                foreach (var user in userData)
                {
                    var existingUser = await db.Members.FindAsync(user.Key.DiscordId, user.Key.GuildId);
                    if (existingUser != null)
                    {
                        existingUser.XP = user.Value;
                        db.Members.Update(existingUser);
                    }
                    else
                    {
                        db.Members.Add(user.Key);
                    }
                }
                await db.SaveChangesAsync();
                return Result<bool, SystemError<CleanUp>>.Ok(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error attempting to saving member data on app closing: {ex.Message}");
                Log.Error(ex, "Error attempting to save member data on app closing.");
                return Result<bool, SystemError<CleanUp>>.Err(new SystemError<CleanUp>
                {
                    ErrorMessage = "Failed to save member data on app closing.",
                    ErrorType = Enums.ErrorType.FATAL,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = this
                });
            }
        }
    }
}
