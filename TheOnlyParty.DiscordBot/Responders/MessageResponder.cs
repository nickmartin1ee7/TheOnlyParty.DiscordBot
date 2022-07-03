
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

using TheOnlyParty.DiscordBot.Services;

namespace TheOnlyParty.DiscordBot.Responders;

public class MessageCreateResponder : IResponder<IMessageCreate>
{
    private readonly ILogger<MessageCreateResponder> _logger;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly MlService _mlService;

    public MessageCreateResponder(ILogger<MessageCreateResponder> logger,
        IDiscordRestChannelAPI channelApi,
        MlService mlService)
    {
        _logger = logger;
        _channelApi = channelApi;
        _mlService = mlService;
    }

    public async Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct = default)
    {
        var messageResult = await _channelApi.GetChannelMessageAsync(gatewayEvent.ChannelID, gatewayEvent.ID, ct);

        if (!messageResult.IsSuccess || string.IsNullOrWhiteSpace(messageResult.Entity.Content))
            return Result.FromSuccess();

        var result = await _mlService.PredictAsync(gatewayEvent.Content, ct);

        if (!result.IsSuccess || result.Result is null)
        {
            _logger.LogError("Failed to use ML Web API prediction");
            return Result.FromSuccess();
        }

        // TODO: Store in db
        _logger.LogTrace("{authorName} ({authorId}): {messageContent} ({prediction} - {confidence:P})",
            messageResult.Entity.Author.Username,
            messageResult.Entity.Author.ID,
            messageResult.Entity.Content,
            result.Result.Positive ? "Positive" : "Negative",
            result.Result.Confidence);

        return Result.FromSuccess();
    }
}
