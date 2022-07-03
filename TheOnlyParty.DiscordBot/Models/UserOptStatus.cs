using System.ComponentModel.DataAnnotations;

namespace TheOnlyParty.DiscordBot.Models;

public record UserOptStatus
{
    [Key]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string UserId { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public bool Enabled { get; set; }
}