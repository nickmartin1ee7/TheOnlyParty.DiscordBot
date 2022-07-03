using System.ComponentModel.DataAnnotations;

namespace TheOnlyParty.DiscordBot.Models;

public record UserOptStatus
{
    [Key]
    public string UserId { get; set; }
    public bool Enabled { get; set; }
}