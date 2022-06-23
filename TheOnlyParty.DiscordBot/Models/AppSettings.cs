namespace TheOnlyParty.DiscordBot.Models;

public class AppSettings
{
    public string? DiscordToken { get; set; }
    public string? DiscordStatus { get; set; }
    public string? DiscordDebugGuildId { get; set; }
    public string? LoggingUri { get; set; }
    public string? LoggingKey { get; set; }
    public string? ReplUri { get; set; }
}