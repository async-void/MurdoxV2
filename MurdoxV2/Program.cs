#region USINGS
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Factories;
using MurdoxV2.Handlers;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using MurdoxV2.Services;
using MurdoxV2.Utilities.OnAppClosing;
using MurdoxV2.Utilities.Timestamp;
using Quartz;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;
#endregion

namespace MurdoxV2
{
    internal class Program
    {
        private static readonly Dictionary<ServerMember, int> _userRank = [];
       
        static async Task Main(string[] args)
        {
            var configService = new ConfigurationDataServiceProvider();
            var token = await configService.GetDiscordTokenAsync();
            var conStr = await configService.GetConnectionStringsAsync();
            TimestampDataProvider.SetBotTimestamp();

            if (!token.IsOk)
            {
                Log.Information($"Error retrieving token: {token.Error.ErrorMessage}");
                return;
            }

            var intents = TextCommandProcessor.RequiredIntents | SlashCommandProcessor.RequiredIntents | DiscordIntents.All;

            var logger = Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Error)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code, outputTemplate: "[{Timestamp:MM-dd-yyyy hh:mm:ss.fff tt zzz} {SourceContext} {Level:u3}] {Message:lj}{NewLine}")
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "TextFiles", "Logs", "bot_logs.txt"), rollingInterval: RollingInterval.Day,
                 outputTemplate: "[{Timestamp:MM-dd-yyyy hh:mm:ss.fff tt zzz} {SourceContext} {Level:u3}] {Message:lj}{NewLine}")
                .CreateLogger();

            await Host.CreateDefaultBuilder()
                .UseSerilog()
                .UseConsoleLifetime()

            #region CONFIGURE SERVICES
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging(logging => logging.ClearProviders().AddSerilog(logger));

                    services.AddHostedService<BotService>()
                        .AddDiscordClient(token.Value, intents)
                        .AddCommandsExtension((options, config) =>
                        {
                            config.AddCommands(Assembly.GetExecutingAssembly());
                        });

                    services.AddSingleton<IDbContextFactory<AppDbContext>>(new AppDbContextFactory(conStr.Value.ConnectionStrings!.Murdox!));
                    services.AddSingleton<IMemberData, MemberDataServiceProvider>();
                    services.AddSingleton<IReminderData, ReminderServiceDataProvider>();
                    services.AddSingleton<IReminder, ReminderService>();
                    services.AddSingleton<IFact, FactDataServiceProvider>();

                    #region QUARTS
                    services.AddQuartz(q =>
                    {
                        q.ScheduleJob<ReminderJob>(trigger => trigger
                            .WithIdentity("ReminderJob", "Murdox")
                            .StartAt(DateTimeOffset.UtcNow.AddSeconds(10))
                            .WithSimpleSchedule(x => x
                                .WithInterval(TimeSpan.FromSeconds(10))
                                .RepeatForever()));
                        //q.ScheduleJob<DailyFactJob>(trigger => trigger
                        //    .WithIdentity("DailyFactJob", "Murdox")
                        //    .StartAt(DateTimeOffset.UtcNow.AddSeconds(10))
                        //    .WithSimpleSchedule(x => x
                        //        .WithInterval(TimeSpan.FromSeconds(10))
                        //        .RepeatForever()));
                    });

                    services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
                    #endregion


                    #region EVENT HANDLERS
                    services.ConfigureEventHandlers(

                    #region SESSION CREATED
                    e => e.HandleSessionCreated((client, args) =>
                    {
                        Log.Information($"Discord Session Created...");
                        return Task.CompletedTask;
                    })
                    #endregion

                    .AddEventHandlers<InteractionEventHandler>(ServiceLifetime.Singleton)

                    #region MESSAGE CREATED
                    .HandleMessageCreated(async (client, args) =>
                    {
                        if (args.Author.IsBot) return;
                           
                    })
                    #endregion

                    #region SOCKETS
                    .HandleSocketClosed((s, e) =>
                    {
                        Log.Information($"Socket closed with code: {e.CloseCode} reason: {e.CloseMessage}");
                        return Task.CompletedTask;
                    })
                    .HandleSocketOpened((s, e) =>
                    {
                        Log.Information($"Socket opened successfully.");
                        return Task.CompletedTask;
                    })
                    #endregion

                    #region GUILD ADDED
                    .HandleGuildCreated(async (client, args) =>//TODO: this may need to be async
                    {
                        var db = new AppDbContextFactory(conStr.Value.ConnectionStrings.Murdox!).CreateDbContext();
                        var guild = await db.Guilds
                            .AsNoTracking()
                            .Where(g => g.GuildId.ToString() == args.Guild.Id.ToString())
                            .FirstOrDefaultAsync();
                        if (guild is not null) { }
                        else
                        {
                            var _guild = new Server()
                            {
                                GuildId = args.Guild.Id.ToString(),
                                GuildName = args.Guild.Name,
                                OwnerId = args.Guild.OwnerId.ToString(),
                                OwnerUsername = args.Guild.GetMemberAsync(args.Guild.OwnerId).ToString() ?? "Unknown",
                                NotificationChannelId = args.Guild.GetDefaultChannel()!.Id.ToString(),
                                CreatedAt = DateTimeOffset.UtcNow,
                                EnableFacts = false,
                                Members = [.. args.Guild.Members.Select(m => new ServerMember
                                {
                                    DiscordId = m.Value.Id.ToString(),
                                    GuildId = args.Guild.Id.ToString(),
                                    GlobalUsername = m.Value.Username,
                                    Nickname = m.Value.Nickname ?? "No Nickname",
                                    AvatarUrl = m.Value.AvatarUrl,
                                    Discriminator = m.Value.Discriminator,
                                    IsBot = m.Value.IsBot,
                                    JoinedAt = m.Value.JoinedAt,
                                    UserStatus = m.Value.Presence?.Status.ToString(),
                                    XP = 0,
                                    MessageCount = 0,
                                    Bank = new Bank()
                                    {
                                        Balance = 0,
                                        Deposit_Amount = 0,
                                        Withdraw_Amount = 0,
                                        Deposit_Timestamp = DateTimeOffset.UtcNow,
                                        Withdraw_Timestamp = DateTimeOffset.UtcNow,
                                    }
                                })]
                            };
                            await db.Guilds.AddAsync(_guild);
                            await db.SaveChangesAsync();

                        }
                            Log.Information($"Guild added: {args.Guild.Name} ({args.Guild.Id})");
                    })
                    #endregion

                    #region GUILD DELETED
                    .HandleGuildDeleted((client, args) =>
                    {
                        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss.fff tt zzz");
                        var db = new AppDbContextFactory(conStr.Value.ConnectionStrings.Murdox!).CreateDbContext();
                        Log.Information($"Guild Removed: {args.Guild.Name} ({args.Guild.Id})");
                        return Task.CompletedTask;
                    })
                    #endregion

                    #region ZOMBIED
                    .HandleZombied((client, args) =>
                    {
                        Log.Information($"Discord Zombied...");
                        return Task.CompletedTask;
                    })
                    #endregion
                );
                #endregion
                })
                .RunConsoleAsync();
            
            var cleanuo = new CleanUp(new AppDbContextFactory(conStr.Value.ConnectionStrings!.Murdox!));
            await cleanuo.SaveMemberDataOnCloseAsync(_userRank);
            await Log.CloseAndFlushAsync();

            #endregion
        }
    }
}
