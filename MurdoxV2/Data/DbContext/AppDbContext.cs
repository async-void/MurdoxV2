using Microsoft.EntityFrameworkCore;
using MurdoxV2.Models;
namespace MurdoxV2.Data.DbContext
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
    {
        public DbSet<ServerMember> Members { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
