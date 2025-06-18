using DSharpPlus;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Factories;
using MurdoxV2.Services;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace MurdoxV2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var configService = new ConfigurationDataServiceProvider();
            var token = await configService.GetDiscordTokenAsync();
            var conStr = await configService.GetConnectionStringsAsync();

            if (!token.IsOk)
            {
                Log.Information($"Error retrieving token: {token.Error.ErrorMessage}");
                return;
            }

            var intents = TextCommandProcessor.RequiredIntents | SlashCommandProcessor.RequiredIntents | DiscordIntents.All;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Error)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {SourceContext} {Level:u3}] {Message:lj}{NewLine}")
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "TextFiles", "Logs", "bot_logs.txt"), rollingInterval: RollingInterval.Day,
                 outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {SourceContext} {Level:u3}] {Message:lj}{NewLine}")
                .CreateLogger();

            await Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseConsoleLifetime()
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging(logging => logging.ClearProviders().AddSerilog());
                    services.AddHostedService<BotService>()
                        .AddDiscordClient(token.Value, intents);
                    services.AddSingleton<IDbContextFactory<AppDbContext>>(new AppDbContextFactory(conStr.Value.ConnectionStrings!.Murdox!));
                })
                
                .RunConsoleAsync();

            await Log.CloseAndFlushAsync();
        }
    }
}
