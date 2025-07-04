using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Enums;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using MurdoxV2.Extensions;
namespace MurdoxV2.Services
{
    public class FactDataServiceProvider(IDbContextFactory<AppDbContext> dbFactory) : IFact
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

        #region GET FACTS FROM DB OR WEBSITE
        public async Task<Result<List<Fact>, SystemError<FactDataServiceProvider>>> GetFactsAsync()
        {
            using var db = _dbFactory.CreateDbContext();
            var facts = await db.Facts
                                .AsNoTracking()
                                .ToListAsync();
            if (facts.Count > 0)
            {
                return Result<List<Fact>, SystemError<FactDataServiceProvider>>.Ok(facts);
            }
            else
            {
                var factsList = new List<Fact>();
                string url = "https://www.thefactsite.com/1000-interesting-facts/";
                var web = new HtmlWeb();
                var doc = web.Load(url);
                var factNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'post-176221')]//p");

                if (factNodes != null)
                {
                    foreach (var node in factNodes)
                    {
                        var fact = new Fact
                        {
                            FactUrl = url,
                            Content = node.InnerText.Sanitize(),
                            Category = "General Knowledge"
                        };
                        factsList.Add(fact);
                    }
                    await db.Facts.AddRangeAsync(factsList);
                    await db.SaveChangesAsync();

                    if (factsList.Count > 0)
                        return Result<List<Fact>, SystemError<FactDataServiceProvider>>.Ok(factsList);
                    else
                    {
                        return Result<List<Fact>, SystemError<FactDataServiceProvider>>.Err(new SystemError<FactDataServiceProvider>
                        {
                            ErrorMessage = "No Facts Found",
                            ErrorType = ErrorType.NOTFOUND,
                            CreatedBy = this,
                            CreatedAt = DateTimeOffset.UtcNow
                        });
                    }
                }
                else
                {
                    return Result<List<Fact>, SystemError<FactDataServiceProvider>>.Err(new SystemError<FactDataServiceProvider>
                    {
                        ErrorMessage = "No Facts Found.",
                        ErrorType = ErrorType.NOTFOUND,
                        CreatedBy = this,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
                }
            }
        }
        #endregion

        #region GET RANDOM FACT
        public async Task<Result<Fact, SystemError<FactDataServiceProvider>>> GetRandomFactAsync()
        {
            using var db = _dbFactory.CreateDbContext();
            var randomFact = await db.Facts
                                     .FromSqlRaw("SELECT * FROM \"Facts\" TABLESAMPLE SYSTEM (0.01) LIMIT 1")
                                     .FirstOrDefaultAsync();
            if (randomFact is not null)
            {
                var count = await db.Facts.CountAsync();
                var index = new Random().Next(count);
                var randomEntity = await db.Facts
                    .OrderBy(e => e.Id)
                    .Skip(index)
                    .FirstOrDefaultAsync();

                return Result<Fact, SystemError<FactDataServiceProvider>>.Ok(randomEntity!);
            }
            else
            {
                _ = await GetFactsAsync();
                var count = await db.Facts.CountAsync();
                var index = new Random().Next(count);
                var randomEntity = await db.Facts
                    .OrderBy(e => e.Id)
                    .Skip(index)
                    .FirstOrDefaultAsync();

                return Result<Fact, SystemError<FactDataServiceProvider>>.Ok(randomEntity!);
            }
        }
        #endregion

    }
}
