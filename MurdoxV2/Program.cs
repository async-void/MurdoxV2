#region USINGS
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MurdoxV2.Cache.TicketCache;
using MurdoxV2.Coordinators;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Enrichers;
using MurdoxV2.Factories;
using MurdoxV2.Features.ScamDetection;
using MurdoxV2.Handlers;
using MurdoxV2.Helpers;
using MurdoxV2.Interfaces;
using MurdoxV2.MessageQueue.SystemNotification;
using MurdoxV2.Models;
using MurdoxV2.QuartzJobs;
using MurdoxV2.RoleCheck;
using MurdoxV2.Services;
using MurdoxV2.Services.Builders;
using MurdoxV2.Services.MessageCache;
using MurdoxV2.Services.Tags;
using MurdoxV2.Services.Tickets;
using MurdoxV2.Services.UrlServices;
using MurdoxV2.Services.Welcomer;
using MurdoxV2.Services.Welcomer.Guild;
using MurdoxV2.Services.Welcomer.Member;
using MurdoxV2.SlashCommands.Moderation;
using MurdoxV2.Utilities.OnAppClosing;
using MurdoxV2.Utilities.Timestamp;
using Quartz;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;
using System.Text.Json;
#endregion

namespace MurdoxV2
{
    internal class Program
    {
        private static readonly Dictionary<ServerMember, int> _userRank = [];
       
        static async Task Main(string[] eventArgs)
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
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "config.json"), optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .Enrich.With(new ColoredSourceContextEnricher())
                .Enrich.With(new FourLetterLevelEnricher())
                .Enrich.With(new RenderedMessageEnricher())
                .ReadFrom.Configuration(configuration)
                .WriteTo.Console(
                    theme: AnsiConsoleTheme.Code,
                    outputTemplate: "[{Timestamp:M-d-yyyy h:mm:ss.fff tt}] [{ColoredSourceContextPadded}] {ColoredLevel} {ColoredMessage}{NewLine}{Exception}")
                .CreateLogger();

            await Host.CreateDefaultBuilder()
                .UseSerilog()
                .UseConsoleLifetime()

            #region CONFIGURE SERVICES
                .ConfigureServices((context, services) =>
                {
                    services.Configure<ConfigJson>(configuration.GetSection("Discord"));
                    services.AddHostedService<BotService>()
                        .AddDiscordClient(token.Value, intents)
                        .AddCommandsExtension((options, config) =>
                        {
                            config.AddCommands(Assembly.GetExecutingAssembly());
                            config.AddCheck<SystemNotificationRoleCheck>();
                            //config.AddCommands([typeof(ModerationCommands)]);
                        });

                    var murdox = conStr.Value?.ConnectionStrings?.Murdox
                            ?? throw new InvalidOperationException("Missing Murdox connection string.");
                    services.AddDbContextFactory<AppDbContext>(options =>
                    {
                        options.UseNpgsql(murdox);
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
                    services.AddSingleton(new ScamImageHashConfig(AHashThreshold: 10, DHashThreshold: 10, PHashThreshold: 12));
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
                              .AddEventHandlers<GuildAddedEventHandler>(ServiceLifetime.Singleton)
                              .AddEventHandlers<GuildRemovedEventHandler>(ServiceLifetime.Singleton)
                              .AddEventHandlers<SessionCreatedEventHandler>(ServiceLifetime.Singleton)
                              .AddEventHandlers<SessionResumedEventHandler>(ServiceLifetime.Singleton)
                              .AddEventHandlers<ComponentInteractionHandler>(ServiceLifetime.Singleton)
                              .AddEventHandlers<MessageDeletedEventHandler>(ServiceLifetime.Singleton)
                              //.AddEventHandlers<InteractionEventHandler>(ServiceLifetime.Singleton)

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
            
            var cleanup = new CleanUp(new AppDbContextFactory(conStr.Value.ConnectionStrings!.Murdox!));
            await cleanup.SaveMemberDataOnCloseAsync(_userRank);
            await Log.CloseAndFlushAsync();

            #endregion

            //var urlRegex = new Regex(@"\b(?:[a-z][a-z0-9+\-.]*://|www\.)[^\s<>()]+",
            //  RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}
