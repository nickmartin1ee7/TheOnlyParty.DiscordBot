using System.ComponentModel;

using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Gateway;
using Remora.Results;

namespace TheOnlyParty.DiscordBot.Commands
{
    public class AdminCommandGroup : LoggedCommandGroup<AdminCommandGroup>
    {
        private readonly DiscordGatewayClient _discordGatewayClient;
        private readonly FeedbackService _feedbackService;

        public AdminCommandGroup(ICommandContext ctx,
            ILogger<AdminCommandGroup> logger,
            IDiscordRestGuildAPI guildApi,
            DiscordGatewayClient discordGatewayClient,
            IDiscordRestChannelAPI channelApi,
            FeedbackService feedbackService)
            : base(ctx, logger, guildApi, channelApi)
        {
            _discordGatewayClient = discordGatewayClient;
            _feedbackService = feedbackService;
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

                var updateCommand = new UpdatePresence(new ClientStatus().Desktop.Value, false, null, new IActivity[]
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
