using Microsoft.Extensions.DependencyInjection;

using Remora.Commands.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Hosting.Extensions;

using Serilog;

using TheOnlyParty.DiscordBot;
using TheOnlyParty.DiscordBot.Commands;

var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

var settings = configuration
    .GetSection(nameof(AppSettings))
    .Get<AppSettings>();

ConfigureLogger(configuration, settings);

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog(Log.Logger)
    .AddDiscordService(_ => settings.Token)
    .ConfigureServices(services =>
    {
        services
            .AddDiscordCommands(true)
            .AddSingleton(configuration)
            .AddSingleton(settings)
            .AddCommandTree()
            .WithCommandGroup<UserCommandGroup>()
            .Finish()
            .AddHostedService<Worker>()
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

await host.RunAsync();

static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    Log.Logger.Fatal(e.ExceptionObject as Exception, "Unhandled exception! Is Terminating? {isTerminating}", e.IsTerminating);
    Log.CloseAndFlush();
}

static void ConfigureLogger(IConfigurationRoot configuration, AppSettings settings)
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.Seq(
            serverUrl: settings.LoggingUri,
            apiKey: settings.LoggingKey)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}