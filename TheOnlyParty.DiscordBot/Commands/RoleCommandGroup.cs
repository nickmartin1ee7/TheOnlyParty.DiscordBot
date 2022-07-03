using System.ComponentModel;

using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Extensions.Embeds;
using Remora.Discord.Gateway;
using Remora.Rest.Core;
using Remora.Results;

using TheOnlyParty.DiscordBot.DbContexts;
using TheOnlyParty.DiscordBot.Models;
using TheOnlyParty.DiscordBot.Services;

namespace TheOnlyParty.DiscordBot.Commands
{
    public class RoleCommandGroup : LoggedCommandGroup<UserCommandGroup>
    {
        private readonly DiscordGatewayClient _discordGatewayClient;
        private readonly FeedbackService _feedbackService;
        private readonly DiscordDbContext _discordDbContext;
        private readonly ReplService _replService;

        public RoleCommandGroup(ICommandContext ctx,
            ILogger<UserCommandGroup> logger,
            IDiscordRestGuildAPI guildApi,
            DiscordGatewayClient discordGatewayClient,
            IDiscordRestChannelAPI channelApi,
            FeedbackService feedbackService,
            DiscordDbContext discordDbContext,
            ReplService replService)
            : base(ctx, logger, guildApi, channelApi)
        {
            _discordGatewayClient = discordGatewayClient;
            _feedbackService = feedbackService;
            _discordDbContext = discordDbContext;
            _replService = replService;
        }

        [Command(nameof(GetSentiment))]
        [CommandType(ApplicationCommandType.ChatInput)]
        [DiscordDefaultMemberPermissions(DiscordPermission.Administrator)]
        [Ephemeral]
        [Description("Get the historical sentiment of a user")]
        public async Task<IResult> GetSentiment([Description("User ID")] string userId)
        {
            try
            {
                await LogCommandUsageAsync(nameof(GetSentiment), userId);

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

        [Command(nameof(ResetSentiment))]
        [CommandType(ApplicationCommandType.ChatInput)]
        [DiscordDefaultMemberPermissions(DiscordPermission.Administrator)]
        [Ephemeral]
        [Description("Resets the historical sentiment of a user")]
        public async Task<IResult> ResetSentiment([Description("User ID")] string userId)
        {
            try
            {
                await LogCommandUsageAsync(nameof(ResetSentiment), userId);

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

        [Command(nameof(ToggleSentiment))]
        [CommandType(ApplicationCommandType.ChatInput)]
        [DiscordDefaultMemberPermissions(DiscordPermission.Administrator)]
        [Ephemeral]
        [Description("Toggle Opt-in/Opt-out of sentiment analysis for you")]
        public async Task<IResult> ToggleSentiment()
        {
            try
            {
                var authorId = _ctx.User.ID.ToString();

                await LogCommandUsageAsync(nameof(ToggleSentiment), authorId);

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

        [Command(nameof(ListUserReports))]
        [CommandType(ApplicationCommandType.ChatInput)]
        [DiscordDefaultMemberPermissions(DiscordPermission.Administrator)]
        [Ephemeral]
        [Description("List all the user reports")]
        public async Task<IResult> ListUserReports()
        {
            try
            {
                await LogCommandUsageAsync(nameof(ListUserReports));

                if (!_discordDbContext.UserReports.Any())
                {
                    var errReply = await _feedbackService.SendContextualErrorAsync("No user reports found");
                    _logger.LogDebug("No user reports found");

                    return errReply.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(errReply);
                }

                var userReports = _discordDbContext.UserReports
                    .AsEnumerable()
                    .OrderByDescending(ur => ur.PositivityRate)
                    .ToArray();

                _logger.LogDebug("Query for user reports successful");

                var embedBuilder = new EmbedBuilder
                {
                    Title = "User Reports",
                    Description = $"There are {userReports.Length} user reports",
                };

                foreach (var userReport in userReports)
                {
                    // get username from userid
                    _ = Snowflake.TryParse(userReport.UserId, out var userId);
                    var user = await _guildApi.GetGuildMemberAsync(_ctx.GuildID.Value, userId!.Value, CancellationToken);

                    embedBuilder.AddField(
                        $"{user.Entity.Nickname.Value} ({user.Entity.User.Value.Username})",
                        $"{userReport.PositivityRate:P} ({userReport.TotalMessages} messages)",
                        true);
                }

                _logger.LogDebug("Built embed for user reports from guild API");

                var reply = await _feedbackService.SendContextualEmbedAsync(embedBuilder.Build().Entity, ct: CancellationToken);

                _logger.LogDebug("List user reports completed successfully");

                return reply.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(reply);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing user reports");
                throw;
            }
        }

        [Command(nameof(ChangeStatus))]
        [CommandType(ApplicationCommandType.ChatInput)]
        [DiscordDefaultMemberPermissions(DiscordPermission.Administrator)]
        [Description("Change the status of this bot")]
        public async Task<IResult> ChangeStatus([Description("New status")] string statusMessage)
        {
            try
            {
                await LogCommandUsageAsync(nameof(ChangeStatus), statusMessage);

                if (string.IsNullOrWhiteSpace(statusMessage)) return Result.FromSuccess();

                var updateCommand = new UpdatePresence(ClientStatus.Online, false, null, new IActivity[]
                {
                new Activity(statusMessage, ActivityType.Watching)
                });

                _discordGatewayClient.SubmitCommand(updateCommand);

                var reply = await _feedbackService.SendContextualSuccessAsync("Status changed!", ct: CancellationToken);

                return reply.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(reply);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing status");
                throw;
            }
        }
    }
}
