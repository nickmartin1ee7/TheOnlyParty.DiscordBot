using System.Text.Json;

namespace TheOnlyParty.DiscordBot.Extensions;

public static class StringExtensions
{
    public static T? FromJson<T>(this string json) => JsonSerializer.Deserialize<T>(json);
}
