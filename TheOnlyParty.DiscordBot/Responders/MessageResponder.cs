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
    private readonly DiscordDbContext _discordDbContext;
    private readonly AppSettings _settings;
    private readonly MlService _mlService;

    public MessageCreateResponder(ILogger<MessageCreateResponder> logger,
        IDiscordRestChannelAPI channelApi,
        DiscordDbContext discordDb,
        AppSettings settings,
        MlService mlService)
    {
        _logger = logger;
        _channelApi = channelApi;
        _discordDbContext = discordDb;
        _settings = settings;
        _mlService = mlService;
    }

    public async Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.Author.IsBot.HasValue && gatewayEvent.Author.IsBot.Value) return Result.FromSuccess();

        var authorId = gatewayEvent.Author.ID.ToString();

        var userOptStatus = _discordDbContext.UserOptStatus.FirstOrDefault(u => u.UserId == authorId);

        if (userOptStatus is null)
        {
            userOptStatus = new UserOptStatus
            {
                UserId = authorId,
                Enabled = true
            };

            _discordDbContext.UserOptStatus.Add(userOptStatus);
        }
        else if (!userOptStatus.Enabled)
        {
            return Result.FromSuccess();
        }

        _logger.LogDebug("Message Created by {authorName} ({authorId})", gatewayEvent.Author.Username, authorId);

        var messageResult = await _channelApi.GetChannelMessageAsync(gatewayEvent.ChannelID, gatewayEvent.ID, ct);

        if (!messageResult.IsSuccess || string.IsNullOrWhiteSpace(messageResult.Entity.Content))
        {
            _logger.LogError("Failed to get message content! {errorMessage}",
                messageResult.Error?.Message ?? "Error message N/A");
            return Result.FromError(messageResult);
        }

        _logger.LogDebug("Got content for message ({messageId})", messageResult.Entity.ID);

        var mlResult = await _mlService.PredictAsync(messageResult.Entity.Content, ct);

        if (!mlResult.IsSuccess || mlResult.Result is null)
        {
            _logger.LogError("Failed to use ML Web API prediction");
            return Result.FromSuccess();
        }

        _logger.LogInformation("{authorName} ({authorId}): {messageContent} ({prediction} - {confidence:P})",
            messageResult.Entity.Author.Username,
            authorId,
            messageResult.Entity.Content,
            mlResult.Result.Positive ? "Positive" : "Negative",
            mlResult.Result.Confidence);

        if (mlResult.Result.Confidence < _settings.MlConfidenceThreshold)
        {
            _logger.LogDebug("Confidence threshold not met ({confidence:P}). Threshold is {threshold:P}",
                mlResult.Result.Confidence,
                _settings.MlConfidenceThreshold);
            
            return Result.FromSuccess();
        }

        var existingUser = _discordDbContext.UserReports.FirstOrDefault(ur => ur.UserId == authorId);

        if (existingUser is null)
        {
            _logger.LogDebug("Creating new User Report for {authorId}", authorId);

            _discordDbContext.UserReports.Add(new UserReport
            {
                UserId = authorId,
                TotalMessages = 1,
                PositiveMessages = mlResult.Result.Positive ? 1 : 0,
                NegativeMessages = mlResult.Result.Positive ? 0 : 1
            });
        }
        else
        {
            _logger.LogDebug("Updating User Report for {authorId}", authorId);
            existingUser.TotalMessages++;
            existingUser.PositiveMessages += mlResult.Result.Positive ? 1 : 0;
            existingUser.NegativeMessages += mlResult.Result.Positive ? 0 : 1;
            _discordDbContext.UserReports.Update(existingUser);
        }

        await _discordDbContext.SaveChangesAsync();

        _logger.LogDebug("Message Created handled successfully");

        return Result.FromSuccess();
    }
}
