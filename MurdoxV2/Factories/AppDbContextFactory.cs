using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Factories
{
    public class AppDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private readonly string _conStr;

        public AppDbContextFactory(string conStr)
        {
            _conStr = conStr;
        }
        public AppDbContext CreateDbContext()
        { 
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(_conStr);
            return new AppDbContext(optionsBuilder.Options); 
        }
    }
}
