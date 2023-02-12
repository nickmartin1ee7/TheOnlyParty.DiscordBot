using System.Runtime.CompilerServices;

using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;

using TheOnlyParty.DiscordBot.Extensions;

namespace TheOnlyParty.DiscordBot.Commands;

public class LoggedCommandGroup<TCommandGroup> : CommandGroup
    where TCommandGroup : class
{
    protected readonly ICommandContext _ctx;
    protected readonly ILogger<TCommandGroup> _logger;
    protected readonly IDiscordRestGuildAPI _guildApi;
    protected readonly IDiscordRestChannelAPI _channelApi;

    public LoggedCommandGroup(ICommandContext ctx, ILogger<TCommandGroup> logger, IDiscordRestGuildAPI guildApi, IDiscordRestChannelAPI channelApi)
    {
        _ctx = ctx;
        _logger = logger;
        _guildApi = guildApi;
        _channelApi = channelApi;
    }

    protected async Task LogCommandUsageAsync([CallerMemberName] string callerMethodName = "", params string[] commandArguments)
    {
        var c = _ctx as InteractionContext;

        var guildName = await _guildApi.GetGuildAsync(c!.Interaction.GuildID.Value, ct: CancellationToken);

        var channelName = await _channelApi.GetChannelAsync(c.Interaction.ChannelID.Value, ct: CancellationToken);

        _logger.LogInformation("{commandName} triggered by {userName} ({userId}) in #{channel} ({channelId}); {guildName} ({guildId}); Message: {message}",
            callerMethodName,
            c.Interaction.Member.Value.User.Value.ToFullUsername(),
            c.Interaction.Member.Value.User.Value.ID,
            channelName.Entity.Name.Value,
            c.Interaction.ChannelID,
            guildName.Entity.Name,
            c.Interaction.GuildID.Value,
            commandArguments.Any() ? string.Join(' ', commandArguments) : "None");
    }
}