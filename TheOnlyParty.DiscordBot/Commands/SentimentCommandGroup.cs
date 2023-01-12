using System.ComponentModel;

using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Extensions.Embeds;
using Remora.Rest.Core;
using Remora.Results;

using TheOnlyParty.DiscordBot.DbContexts;
using TheOnlyParty.DiscordBot.Extensions;
using TheOnlyParty.DiscordBot.Models;

namespace TheOnlyParty.DiscordBot.Commands
{
    public class SentimentCommandGroup : LoggedCommandGroup<SentimentCommandGroup>
    {
        private readonly FeedbackService _feedbackService;
        private readonly DiscordDbContext _discordDbContext;

        public SentimentCommandGroup(ICommandContext ctx,
            ILogger<SentimentCommandGroup> logger,
            IDiscordRestGuildAPI guildApi,
            IDiscordRestChannelAPI channelApi,
            FeedbackService feedbackService,
            DiscordDbContext discordDbContext)
            : base(ctx, logger, guildApi, channelApi)
        {
            _feedbackService = feedbackService;
            _discordDbContext = discordDbContext;
        }

        [Command(nameof(Sentiment))]
        [CommandType(ApplicationCommandType.ChatInput)]
        [DiscordDefaultMemberPermissions(DiscordPermission.Administrator)]
        [Ephemeral]
        [Description("Get the historical sentiment of a user")]
        public async Task<IResult> Sentiment([Description("User ID")] string? userId = null)
        {
            var c = _ctx as InteractionContext;

            userId ??= c!.Interaction.User.Value.ID.ToString();

            try
            {
                await LogCommandUsageAsync(nameof(Sentiment), userId);

                if (string.IsNullOrWhiteSpace(userId)) return Result.FromSuccess();

                var user = _discordDbContext.UserReports.FirstOrDefault(ur => ur.UserId == userId);

                if (user is null)
                {
                    var reply = await _feedbackService.SendContextualErrorAsync("User not found");

                    return reply.IsSuccess
                        ? Result.FromSuccess()
                        : Result.FromError(reply);
                }
                else
                {
                    var reply = await _feedbackService.SendContextualSuccessAsync($"{user}", ct: CancellationToken);

                    return reply.IsSuccess
                        ? Result.FromSuccess()
                        : Result.FromError(reply);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sentiment");
                throw;
            }
        }

        [Command(nameof(SentimentReset))]
        [CommandType(ApplicationCommandType.ChatInput)]
        [DiscordDefaultMemberPermissions(DiscordPermission.Administrator)]
        [Ephemeral]
        [Description("Resets the historical sentiment of a user")]
        public async Task<IResult> SentimentReset([Description("User ID")] string userId)
        {
            try
            {
                await LogCommandUsageAsync(nameof(SentimentReset), userId);

                if (string.IsNullOrWhiteSpace(userId)) return Result.FromSuccess();

                var user = _discordDbContext.UserReports.FirstOrDefault(ur => ur.UserId == userId);

                if (user is null)
                {
                    var reply = await _feedbackService.SendContextualErrorAsync("User has no data");

                    return reply.IsSuccess
                        ? Result.FromSuccess()
                        : Result.FromError(reply);
                }
                else
                {
                    user.TotalMessages = 0;
                    user.PositiveMessages = 0;
                    user.NegativeMessages = 0;

                    _discordDbContext.UserReports.Update(user);
                    await _discordDbContext.SaveChangesAsync();

                    var reply = await _feedbackService.SendContextualSuccessAsync($"User reset: {user}", ct: CancellationToken);

                    return reply.IsSuccess
                        ? Result.FromSuccess()
                        : Result.FromError(reply);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset sentiment");
                throw;
            }
        }

        [Command(nameof(SentimentToggle))]
        [CommandType(ApplicationCommandType.ChatInput)]
        [Ephemeral]
        [Description("Toggle Opt-in/Opt-out of sentiment analysis for you")]
        public async Task<IResult> SentimentToggle()
        {
            var c = _ctx as InteractionContext;

            try
            {
                var authorId = c!.Interaction.User.Value.ID.ToString();

                await LogCommandUsageAsync(nameof(SentimentToggle), authorId);

                var userOptStatus = _discordDbContext.UserOptStatus.FirstOrDefault(ur => ur.UserId == authorId);

                if (userOptStatus is null)
                {
                    userOptStatus = new UserOptStatus
                    {
                        UserId = authorId,
                        Enabled = false // First use is opt-out
                    };

                    _discordDbContext.UserOptStatus.Add(userOptStatus);

                    _logger.LogDebug("UserOptStatus not found, creating new opt-ed out entry");
                }
                else
                {
                    userOptStatus.Enabled = !userOptStatus.Enabled;
                    _discordDbContext.UserOptStatus.Update(userOptStatus);

                    _logger.LogDebug("UserOptStatus found, toggling opt-in/opt-out");
                }

                if (!userOptStatus.Enabled)
                {
                    var userReport = _discordDbContext.UserReports.FirstOrDefault(ur => ur.UserId == userOptStatus.UserId);

                    if (userReport is not null)
                    {
                        _discordDbContext.UserReports.Remove(userReport); // Delete opt-ed out user report
                    }

                    _logger.LogDebug("UserOptStatus is opt-out, deleting user report");
                }

                await _discordDbContext.SaveChangesAsync();

                var reply = await _feedbackService.SendContextualSuccessAsync($"Your opt status changed: {userOptStatus}",
                        ct: CancellationToken);

                _logger.LogDebug("Opt status changed successfully");

                return reply.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(reply);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while toggling opt status");
                throw;
            }
        }

        [Command(nameof(SentimentReport))]
        [CommandType(ApplicationCommandType.ChatInput)]
        [DiscordDefaultMemberPermissions(DiscordPermission.Administrator)]
        [Ephemeral]
        [Description("List all the user sentiment reports")]
        public async Task<IResult> SentimentReport()
        {
            try
            {
                var c = _ctx as InteractionContext;

                await LogCommandUsageAsync(nameof(SentimentReport));

                if (!_discordDbContext.UserReports.Any())
                {
                    var errReply = await _feedbackService.SendContextualErrorAsync("No user sentiment reports found");
                    _logger.LogDebug("No user sentiment reports found");

                    return errReply.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(errReply);
                }

                var userReports = _discordDbContext.UserReports
                    .AsEnumerable()
                    .OrderByDescending(ur => ur.PositivityRate)
                    .ToArray();

                _logger.LogDebug("Query for user sentiment reports successful");

                var embedBuilder = new EmbedBuilder
                {
                    Title = "User Sentiment Reports"
                };

                var localUserCount = 0;

                foreach (var userReport in userReports)
                {
                    _ = Snowflake.TryParse(userReport.UserId, out var userId);
                    var user = await _guildApi.GetGuildMemberAsync(c!.Interaction.GuildID.Value, userId!.Value, CancellationToken);

                    if (!user.IsSuccess || !user.IsDefined()) continue;

                    localUserCount++;

                    embedBuilder.AddField(
                        user.Entity.Nickname.HasValue && !string.IsNullOrWhiteSpace(user.Entity.Nickname.Value)
                            ? $"{user.Entity.Nickname.Value} ({user.Entity.User.Value.ToFullUsername()})"
                            : $"{user.Entity.User.Value.ToFullUsername()}",
                        $"{userReport.PositivityRate:P} ({userReport.TotalMessages} messages)",
                        true);
                }

                embedBuilder.Description = $"There are {localUserCount} user sentiment reports";

                _logger.LogDebug("Built embed for user sentiment reports from guild API");

                var reply = await _feedbackService.SendContextualEmbedAsync(embedBuilder.Build().Entity, ct: CancellationToken);

                _logger.LogDebug("List user sentiment reports completed successfully");

                return reply.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(reply);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing user sentiment reports");
                throw;
            }
        }
    }
}
