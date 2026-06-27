using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Services.MessageCache;
using Quartz;

namespace MurdoxV2.QuartzJobs
{
    public class DiscordMsgCacheResetJob(DiscordMessageCacheService cacheService, ILogger<DiscordMsgCacheResetJob> logger,
        IDbContextFactory<AppDbContext> dbFactory) : IJob
    {
        private readonly DiscordMessageCacheService _cacheService = cacheService;
        private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
        private readonly ILogger<DiscordMsgCacheResetJob> _logger = logger;

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Message Count: [{count}]", _cacheService.Count);
            //TODO: save msg data to db before clearing the cache
            _cacheService.Clear();
            _logger.LogInformation("Message Cache Reset Complete...");
        }
    }
}
