using DSharpPlus.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MurdoxV2.Services;
using Newtonsoft.Json.Linq;
using Serilog;

namespace MurdoxV2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            await Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime()
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging(logging => logging.ClearProviders().AddSerilog());
                    services.AddHostedService<BotService>()
                        .AddDiscordClient(token, intents);
                })
                .RunConsoleAsync();

            await Log.CloseAndFlushAsync();
        }
    }
}
