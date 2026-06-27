using Microsoft.EntityFrameworkCore;
using MurdoxV2.Features.ScamDetection;
using MurdoxV2.Models;
namespace MurdoxV2.Data.DbContext
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
    {
        public DbSet<ServerMember> Members => Set<ServerMember>();
        public DbSet<Reminder> Reminders => Set<Reminder>();
        public DbSet<Fact> Facts => Set<Fact>();
        public DbSet<Server> Guilds => Set<Server>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<ScamImageRecord> ScamImages => Set<ScamImageRecord>();
        public DbSet<CachedDiscordMessage> CachedMessages => Set<CachedDiscordMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Reminder>()
                .HasOne(x => x.Member)
                .WithMany(m => m.Reminders)
                .HasForeignKey(r => r.ServerMemberId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ScamImageRecord>()
                .HasIndex(x => new { x.AHash, x.DHash, x.PHash })
                .IsUnique();

        }
    }
}
