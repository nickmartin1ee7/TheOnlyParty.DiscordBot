using Remora.Commands.Attributes;
using System.Reflection;

using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.API.Objects;
using TheOnlyParty.DiscordBot.Extensions;
using System.Runtime.CompilerServices;

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
        var guildName = await _guildApi.GetGuildAsync(_ctx.GuildID.Value, ct: CancellationToken);

        var channelName = await _channelApi.GetChannelAsync(_ctx.ChannelID, ct: CancellationToken);

        _logger.LogInformation("{commandName} triggered by {userName} ({userId}) in #{channel} ({channelId}); {guildName} ({guildId}); Message: {message}",
            callerMethodName,
            _ctx.User.ToFullUsername(),
            _ctx.User.ID,
            channelName.Entity.Name.Value,
            _ctx.ChannelID,
            guildName.Entity.Name,
            _ctx.GuildID.Value,
            commandArguments.Any() ? string.Join(' ', commandArguments) : "None");
    }
}