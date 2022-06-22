namespace TheOnlyParty.DiscordBot.Services;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker starting at: {time}", DateTimeOffset.Now);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogTrace("Worker is alive at: {time}", DateTimeOffset.Now);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        _logger.LogInformation("Worker stopping at: {time}", DateTimeOffset.Now);
    }
}