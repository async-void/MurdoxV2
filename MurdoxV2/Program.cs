#region USINGS
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Extensions;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MurdoxV2.Cache.TicketCache;
using MurdoxV2.Coordinators;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Enrichers;
using MurdoxV2.Factories;
using MurdoxV2.Features.AutoLearning;
using MurdoxV2.Features.ScamDetection;
using MurdoxV2.Handlers;
using MurdoxV2.Handlers.Button;
using MurdoxV2.Handlers.Modal;
using MurdoxV2.Helpers;
using MurdoxV2.Interfaces;
using MurdoxV2.MessageQueue.SystemNotification;
using MurdoxV2.Models;
using MurdoxV2.QuartzJobs;
using MurdoxV2.Repositories;
using MurdoxV2.RoleCheck;
using MurdoxV2.Services;
using MurdoxV2.Services.Authorization;
using MurdoxV2.Services.Builders;
using MurdoxV2.Services.Builders.Level;
using MurdoxV2.Services.Builders.Profile;
using MurdoxV2.Services.Level;
using MurdoxV2.Services.MessageCache;
using MurdoxV2.Services.Serializers;
using MurdoxV2.Services.Tags;
using MurdoxV2.Services.Tickets;
using MurdoxV2.Services.UrlServices;
using MurdoxV2.Services.Welcomer;
using MurdoxV2.Services.Welcomer.Guild;
using MurdoxV2.Services.Welcomer.Member;
using MurdoxV2.Utilities.OnAppClosing;
using MurdoxV2.Utilities.Timestamp;
using Quartz;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;
using System.Text.Json;
#endregion

namespace MurdoxV2;

internal class Program
{
    private static readonly Dictionary<ServerMember, int> _userRank = [];
   
    static async Task Main(string[] eventArgs)
    {
        TimestampDataProvider.SetBotTimestamp();

        var intents = TextCommandProcessor.RequiredIntents | SlashCommandProcessor.RequiredIntents | DiscordIntents.All;
        //var configuration = new ConfigurationBuilder()
        //    .SetBasePath(Directory.GetCurrentDirectory())
        //    .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", $"config.{GetOsName().ToLower()}.json"), optional: false, reloadOnChange: true)
        //    .Build();
        //var conStr = configuration.GetConnectionString("murdox");
        //var token = configuration.GetSection("Discord")["token"] ?? throw new InvalidOperationException("Missing Discord token.");
        //var roleIds = configuration.GetSection("Discord:authorization:authorizedRoleIds")
        //    .Get<string[]>()?
        //    .Select(ulong.Parse)
        //    .ToList() ?? throw new InvalidOperationException("Missing authorization role IDs.");
        //var allowAdmins = configuration.GetValue<bool>("Discord:authorization:allowAdmins");
        Log.Logger = new LoggerConfiguration()
            .Enrich.With(new ColoredSourceContextEnricher())
            .Enrich.With(new FourLetterLevelEnricher())
            .Enrich.With(new RenderedMessageEnricher())
            .WriteTo.Console(
                theme: AnsiConsoleTheme.Code,
                outputTemplate: "[{Timestamp:M-d-yyyy h:mm:ss.fff tt}] [{ColoredSourceContextPadded}] {ColoredLevel} {ColoredMessage}{NewLine}{Exception}")
            .CreateLogger();

        await Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
            })
            .UseSerilog()
            .UseConsoleLifetime()

            #region CONFIGURE SERVICES
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(new ConfigSerializationService(Path.Combine(AppContext.BaseDirectory, "Data", "Config", $"config.{GetOsName().ToLower()}.json")));

                var tempProvider = services.BuildServiceProvider();
                var cfg = tempProvider.GetRequiredService<ConfigSerializationService>().Config;

                services.AddHostedService<BotService>()
                    .AddDiscordClient(cfg.Discord!.Token!, intents)
                    .AddCommandsExtension((options, config) =>
                    {
                        config.AddCheck<SystemNotificationRoleCheck>();
                        config.AddCheck<ModerationRoleCheck>();
                        config.AddCommands(Assembly.GetExecutingAssembly()); 
                    })
                    .AddInteractivityExtension();

                services.AddDbContextFactory<AppDbContext>(options =>
                {
                    options.UseNpgsql(cfg.ConnectionStrings!.Murdox);
                    options.UseLoggerFactory(LoggerFactory.Create(builder => { }));
                });
                
                services.AddSingleton<IMemberData, MemberDataServiceProvider>();
                services.AddSingleton<IReminderData, ReminderServiceDataProvider>();
                services.AddSingleton<IReminder, ReminderService>();
                services.AddSingleton<IFact, FactDataServiceProvider>();
                services.AddSingleton<TicketCache>();
                services.AddSingleton<ITicket, TicketDataServiceProvider>();
                services.AddSingleton<TicketCoordinator>();
                services.AddSingleton<IWelcomer, WelcomerProviderService>();
                services.AddSingleton<ITagRepository, TagRepositoryProviderService>();
                services.AddSingleton<TagEmbedBuilderProviderService>();
                services.AddSingleton<IDiscordEmbedBuilder, EmbedBuilderServiceProvider>();
                services.AddSingleton<GuildWelcomeConfig>(provider =>
                {
                    var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "Welcomer", "Guild", "guild_welcome_messages.json"));
                    return JsonSerializer.Deserialize<GuildWelcomeConfig>(json)
                        ?? throw new InvalidOperationException("Failed to load guild welcome config.");

                });
                services.AddSingleton<GuildWelcomeMessageProvider>();

                services.AddSingleton<MemberWelcomeConfig>(provider =>
                {
                    var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "Welcomer", "Member", "member_welcome_messages.json"));
                    return JsonSerializer.Deserialize<MemberWelcomeConfig>(json)
                        ?? throw new InvalidOperationException("Failed to load member welcome config");
                });
                
                services.AddSingleton<MemberWelcomeMessageProvider>();
                services.AddSingleton<IUrlCaptureService, UrlCaptureServiceProvider>();
                services.AddSingleton<IUrlRemovaleService, UrlRemovaleServiceProvider>();
                services.AddSingleton<ISystemNotificationQueue, SystemNotificationQueue>();
                services.AddHostedService<SystemNotificationDispatcher>();
                services.AddSingleton<DiscordMessageCacheService>();
                services.AddSingleton<GhostPingService>();
                services.AddSingleton(new RateLimitHelper<ulong>(TimeSpan.FromSeconds(1)));
                services.AddSingleton<ScamDetectionConfig>();
                services.AddSingleton<IScamDetectionService, HeuristicScamDetectionService>();
                services.AddSingleton<IImageIngestionService, HttpImageIngestionService>();
                services.AddScoped<IScamHashRepository, ScamHashRepository>();
                services.AddSingleton(new ScamImageHashConfig(AHashThreshold: 12, DHashThreshold: 12, PHashThreshold: 18));
                services.AddSingleton<IServerRepository, ServerRepository>();
                services.AddSingleton<AutoLearningService>();
                services.AddSingleton<XpLevelBuilderService>();
                services.AddSingleton<ILevel, LevelService>();
                services.AddSingleton<IProfile,  ProfileService>();
                services.AddSingleton<ProfileImageBuilderService>();
                services.AddSingleton(provider =>
                {
                    var configService = provider.GetRequiredService<ConfigSerializationService>();
                    var cfg = configService.Config;
                    return new RoleAuthorizationService(cfg.Discord!.Authorization!.AuthorizedRoleIds, cfg.Discord.Authorization.AllowAdmins);
                });
                
                //BUTTON HANDLERS
                services.AddSingleton<IButtonHandler, DonateButtonHandler>();
                services.AddSingleton<IButtonHandler, PaginationButtonHandler>();
                services.AddSingleton<IButtonHandler, AutoLearnButtonHandler>();
                services.AddSingleton<IButtonHandler, ConfigureButtonHandler>();
                services.AddSingleton<IButtonHandler, PurgeButtonHandler>();
                services.AddSingleton<IButtonHandler, BanButtonHandler>();
                services.AddSingleton<IButtonHandler, WarnButtonHandler>();
                services.AddSingleton<IButtonHandler, UptimeButtonHandler>();
                services.AddSingleton<ButtonRouter>();

                //MODAL HANDLERS
                services.AddSingleton<IModalHandler, HoneypotModalHandler>();
                services.AddSingleton<ModalRouter>();

                #region QUARTS
                services.AddQuartz(q =>
                {
                // Reminder Job
                var remindJobKey = new JobKey("ReminderJob");
                //TODO: fix this job
                q.ScheduleJob<ReminderJob>(trigger => trigger
                    .WithIdentity("ReminderJob", "Murdox")
                    .StartAt(DateTimeOffset.UtcNow.AddSeconds(10))
                    .WithSimpleSchedule(x => x
                        .WithInterval(TimeSpan.FromMinutes(2))
                        .RepeatForever()));

                // Message Cache Reset Job
                var cacheResetJobKey = new JobKey("MessageCacheResetJob");
                q.AddJob<DiscordMsgCacheResetJob>(opts => opts.WithIdentity(cacheResetJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(cacheResetJobKey)
                    .WithIdentity("MessageCacheResetJob-trigger")
                    .WithCronSchedule("0 0 0 * * ?", x => x
                        .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")))
                );

                // Daily Facts Job
                var factsJobKey = new JobKey("DailyFactJob");
                q.AddJob<DailyFactJob>(opts => opts.WithIdentity(factsJobKey)
                                                    .WithDescription("Daily Facts Job"));
                    q.AddTrigger(opts => opts
                        .ForJob(factsJobKey)
                        .WithIdentity("DailyFactsJob-trigger")
                        .WithCronSchedule("0 0 0 * * ?", x => x
                            .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"))));
                });

                services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
                #endregion

                #region EVENT HANDLERS
                services.ConfigureEventHandlers(
                    e => e.AddEventHandlers<MessageCreatedEventHandler>(ServiceLifetime.Singleton)
                          .AddEventHandlers<MemberCreatedEventHandler>(ServiceLifetime.Singleton)
                          .AddEventHandlers<GuildDownloadCompleteEventHandler>(ServiceLifetime.Singleton)
                          .AddEventHandlers<GuildAddedEventHandler>(ServiceLifetime.Singleton)
                          .AddEventHandlers<GuildRemovedEventHandler>(ServiceLifetime.Singleton)
                          .AddEventHandlers<SessionCreatedEventHandler>(ServiceLifetime.Singleton)
                          .AddEventHandlers<SessionResumedEventHandler>(ServiceLifetime.Singleton)
                          .AddEventHandlers<ComponentInteractionHandler>(ServiceLifetime.Singleton)
                          .AddEventHandlers<MessageDeletedEventHandler>(ServiceLifetime.Singleton)
                          .AddEventHandlers<InteractionEventHandler>(ServiceLifetime.Singleton)

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

                #region ZOMBIED
                .HandleZombied((client, eventArgs) =>
                {
                    Log.Information($"Discord Zombied...");
                    return Task.CompletedTask;
                })
                #endregion

                );
                #endregion
            })
            .RunConsoleAsync();

            #endregion
        var newCfg = new ConfigSerializationService(Path.Combine(AppContext.BaseDirectory, "Data", "Config", $"config.{GetOsName().ToLower()}.json"));
        var cleanup = new CleanUp(new AppDbContextFactory(newCfg.Config!.ConnectionStrings!.Murdox!));
        await cleanup.SaveMemberDataOnCloseAsync(_userRank);
        await Log.CloseAndFlushAsync();

        

        //var urlRegex = new Regex(@"\b(?:[a-z][a-z0-9+\-.]*://|www\.)[^\s<>()]+",
        //  RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    static string GetOsName()
    {
        if (OperatingSystem.IsWindows())
            return "Windows";
        else if (OperatingSystem.IsLinux())
            return "Linux";
        else if (OperatingSystem.IsMacOS())
            return "macOS";
        else
            return "Unknown OS";
    }
}
