using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

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

using TheOnlyParty.DiscordBot.Models;
using TheOnlyParty.DiscordBot.Services;

namespace TheOnlyParty.DiscordBot.Commands
{
    public class RoleCommandGroup : LoggedCommandGroup<UserCommandGroup>
    {
        private readonly DiscordGatewayClient _discordGatewayClient;
        private readonly FeedbackService _feedbackService;
        private readonly ReplService _replService;

        public RoleCommandGroup(ICommandContext ctx,
            ILogger<UserCommandGroup> logger,
            IDiscordRestGuildAPI guildApi,
            DiscordGatewayClient discordGatewayClient,
            IDiscordRestChannelAPI channelApi,
            FeedbackService feedbackService,
            ReplService replService)
            : base(ctx, logger, guildApi, channelApi)
        {
            _discordGatewayClient = discordGatewayClient;
            _feedbackService = feedbackService;
            _replService = replService;
        }

        [Command(nameof(ChangeStatus))]
        [CommandType(ApplicationCommandType.ChatInput)]
        [DiscordDefaultMemberPermissions(DiscordPermission.Administrator)]
        [Description("Change the status of this bot")]
        public async Task<IResult> ChangeStatus([Description("New status")] string statusMessage)
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
    }
}
