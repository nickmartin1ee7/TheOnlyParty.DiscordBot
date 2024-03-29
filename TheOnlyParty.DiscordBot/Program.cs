using Microsoft.EntityFrameworkCore;

using OpenAI_API;

using Remora.Commands.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Hosting.Extensions;

using Serilog;

using TheOnlyParty.ClassLibrary.Flight;
using TheOnlyParty.DiscordBot.Commands;
using TheOnlyParty.DiscordBot.DbContexts;
using TheOnlyParty.DiscordBot.Models;
using TheOnlyParty.DiscordBot.Services;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json", true)
    .AddEnvironmentVariables()
    .Build();

var settings = configuration
    .GetSection(nameof(AppSettings))
    .Get<AppSettings>();

ConfigureLogger(configuration, settings);

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog(Log.Logger)
    .AddDiscordService(_ => settings!.DiscordToken!)
    .ConfigureServices(services =>
    {
        services
            .AddDbContext<DiscordDbContext>(options => options
                .UseSqlite("Data Source = /app/Data/discord.db"))
            .AddDiscordCommands(true)
            .AddSingleton(configuration)
            .AddSingleton(settings!)
            .AddSingleton<OpenAIAPI>(_ => new OpenAIAPI(new APIAuthentication(settings.OpenAiApiKey!)))
            .AddTransient(_ => new ReplService(settings!.ReplUri!))
            .AddTransient(_ => new MlService(settings!.MlUri!))
            .AddTransient(sp => new FlightAwareService(sp.GetRequiredService<IHttpClientFactory>(), settings!.FlightAwareApiKey!))
            .AddCommandTree()
            .WithCommandGroup<UserCommandGroup>()
            .WithCommandGroup<AdminCommandGroup>()
            .WithCommandGroup<SentimentCommandGroup>()
            .WithCommandGroup<FlightCommandGroup>()
            .WithCommandGroup<OpenAiCommandGroup>()
            .Finish()
            ;

        var responderTypes = typeof(Program).Assembly
                    .GetExportedTypes()
                    .Where(t => t.IsResponder());

        foreach (var responderType in responderTypes)
        {
            services.AddResponder(responderType);
        }
    })
    .Build();

AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

//host.Services.GetRequiredService<DiscordDbContext>().Database.Migrate();
await host.RunAsync();

static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    Log.Logger.Fatal(e.ExceptionObject as Exception, "Unhandled exception! Is Terminating? {isTerminating}", e.IsTerminating);
    Log.CloseAndFlush();
}

static void ConfigureLogger(IConfigurationRoot configuration, AppSettings? settings)
{
    var loggerConfig = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration);

    if (settings?.LoggingUri is not null)
        loggerConfig.WriteTo.Seq(
                serverUrl: settings.LoggingUri,
                apiKey: settings.LoggingKey);

    Log.Logger = loggerConfig.CreateLogger();
}