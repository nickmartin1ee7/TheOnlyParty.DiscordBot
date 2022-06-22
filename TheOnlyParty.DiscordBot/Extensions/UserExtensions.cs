
using Remora.Discord.API.Abstractions.Objects;

namespace TheOnlyParty.DiscordBot.Extensions;

internal static class UserExtensions
{
    public static string ToFullUsername(this IUser user) => $"{user.Username}#{user.Discriminator}";
}
