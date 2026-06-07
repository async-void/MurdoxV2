using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MurdoxV2.MessageQueue.SystemNotification;
using MurdoxV2.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Dispatchers.LogDispatcher
{
    public sealed class SystemLogDispatcher(DiscordClient client, ILogQueue queue, ILogger<SystemLogDispatcher> logger) : BackgroundService
    {

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("SystemLogDispatcher started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // If queue is empty, yield and continue
                    if (!queue.TryDequeue(out var payload) || payload is null)
                    {
                        await Task.Delay(50, stoppingToken);
                        continue;
                    }

                    await DispatchAsync(payload, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Normal shutdown
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "SystemLogDispatcher encountered an error");
                    await Task.Delay(500, stoppingToken);
                }
            }

            logger.LogInformation("SystemLogDispatcher stopped");
        }

        private async Task DispatchAsync(LogPayload payload, CancellationToken ct)
        {
           //TODO: fix me
        }
    }
    
}
