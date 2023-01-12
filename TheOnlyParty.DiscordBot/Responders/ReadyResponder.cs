using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;

using TheOnlyParty.DiscordBot.Extensions;
using TheOnlyParty.DiscordBot.Models;

namespace TheOnlyParty.DiscordBot.Responders;

public class ReadyResponder : IResponder<IReady>
{
    private readonly ILogger<ReadyResponder> _logger;
    private readonly DiscordGatewayClient _discordGatewayClient;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly SlashService _slashService;
    private readonly AppSettings _settings;

    public ReadyResponder(ILogger<ReadyResponder> logger,
        DiscordGatewayClient discordGatewayClient,
        IDiscordRestGuildAPI guildApi,
        SlashService slashService,
        AppSettings settings)
    {
        _logger = logger;
        _discordGatewayClient = discordGatewayClient;
        _guildApi = guildApi;
        _slashService = slashService;
        _settings = settings;
    }

    public async Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default)
    {
        void UpdatePresence()
        {
            if (string.IsNullOrWhiteSpace(_settings.DiscordStatus)) return;

            var status = new ClientStatus
            {
                Desktop = new Optional<UserStatus>(UserStatus.Online)
            };

            var updateCommand = new UpdatePresence(status.Desktop.Value, false, null, new IActivity[]
            {
                new Activity(_settings.DiscordStatus, ActivityType.Watching)
            });

            _discordGatewayClient.SubmitCommand(updateCommand);
        }

        async Task UpdateSlashCommands(Snowflake? guildId = null)
        {
            var updateResult = await _slashService.UpdateSlashCommandsAsync(guildId, ct: ct);
            var target = guildId is null ? "globally" : $"for {guildId}";

            if (updateResult.IsSuccess)
            {
                _logger.LogInformation("Updated application commands {targetCommandUpdate}", target);
            }
            else
            {
                _logger.LogWarning("Failed to update application commands {targetCommandUpdate}", target);
            }
        }

        async Task<int> CountGuildMembersAsync()
        {
            int userCount = 0;

            foreach (var guild in gatewayEvent.Guilds)
            {
                var guildPreview = await _guildApi.GetGuildPreviewAsync(guild.ID, ct);

                if (!guildPreview.IsSuccess)
                    continue;

                userCount += guildPreview.Entity.ApproximateMemberCount.HasValue
                    ? guildPreview.Entity.ApproximateMemberCount.Value
                    : 0;
            }

            return userCount;
        }

        _logger.LogInformation("Received Ready Event from Gateway");

        _ = Snowflake.TryParse(_settings.DiscordDebugGuildId!, out var guildId);

        UpdatePresence();
        await UpdateSlashCommands(guildId);
        var userCount = await CountGuildMembersAsync();

        _logger.LogInformation(
                "{botUser} is online for {guildCount} guilds and {userCount} users",
                gatewayEvent.User.ToFullUsername(),
                gatewayEvent.Guilds.Count,
                userCount);


        return Result.FromSuccess();
    }
}
