
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

using TheOnlyParty.DiscordBot.DbContexts;
using TheOnlyParty.DiscordBot.Models;
using TheOnlyParty.DiscordBot.Services;

namespace TheOnlyParty.DiscordBot.Responders;

public class MessageCreateResponder : IResponder<IMessageCreate>
{
    private readonly ILogger<MessageCreateResponder> _logger;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly DiscordDbContext _discordDb;
    private readonly MlService _mlService;

    public MessageCreateResponder(ILogger<MessageCreateResponder> logger,
        IDiscordRestChannelAPI channelApi,
        DiscordDbContext discordDb,
        MlService mlService)
    {
        _logger = logger;
        _channelApi = channelApi;
        _discordDb = discordDb;
        _mlService = mlService;
    }

    public async Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct = default)
    {
        var messageResult = await _channelApi.GetChannelMessageAsync(gatewayEvent.ChannelID, gatewayEvent.ID, ct);

        if (!messageResult.IsSuccess || string.IsNullOrWhiteSpace(messageResult.Entity.Content))
            return Result.FromSuccess();

        var mlResult = await _mlService.PredictAsync(gatewayEvent.Content, ct);

        if (!mlResult.IsSuccess || mlResult.Result is null)
        {
            _logger.LogError("Failed to use ML Web API prediction");
            return Result.FromSuccess();
        }

        var authorId = messageResult.Entity.Author.ID.ToString();

        _logger.LogTrace("{authorName} ({authorId}): {messageContent} ({prediction} - {confidence:P})",
            messageResult.Entity.Author.Username,
            authorId,
            messageResult.Entity.Content,
            mlResult.Result.Positive ? "Positive" : "Negative",
            mlResult.Result.Confidence);

        var existingUser = _discordDb.UserReports.FirstOrDefault(ur => ur.UserId == authorId);

        if (existingUser is null)
        {
            await _discordDb.UserReports.AddAsync(new UserReport
            {
                UserId = authorId,
                TotalMessages = 1,
                PositiveMessages = mlResult.Result.Positive ? 1 : 0,
                NegativeMessages = mlResult.Result.Positive ? 0 : 1
            }, ct);
        }
        else
        {
            existingUser.TotalMessages++;
            existingUser.PositiveMessages += mlResult.Result.Positive ? 1 : 0;
            existingUser.NegativeMessages += mlResult.Result.Positive ? 0 : 1;
            _discordDb.UserReports.Update(existingUser);
        }

        await _discordDb.SaveChangesAsync();

        return Result.FromSuccess();
    }
}
