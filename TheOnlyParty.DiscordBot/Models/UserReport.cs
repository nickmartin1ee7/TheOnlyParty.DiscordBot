using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TheOnlyParty.DiscordBot.Models;

public record UserReport
{
    [Key]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string UserId { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public int TotalMessages { get; set; }
    public int PositiveMessages { get; set; }
    public int NegativeMessages { get; set; }

    [JsonIgnore]
    public double PositivityRate => PositiveMessages / (double)TotalMessages;
}
