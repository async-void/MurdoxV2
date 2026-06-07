using Microsoft.Extensions.Logging;
using MurdoxV2.Services.MessageCache;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.QuartzJobs
{
    public class DiscordMsgCacheResetJob(DiscordMessageCacheService cacheService, ILogger<DiscordMsgCacheResetJob> logger) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("Message Count: [{count}]", cacheService.Count);
            cacheService.Clear();
            logger.LogInformation("Message Cache Reset Complete...");
        }
    }
}
