using System.ComponentModel;

using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

using TheOnlyParty.DiscordBot.Services;

namespace TheOnlyParty.DiscordBot.Commands;

public class FlightCommandGroup : LoggedCommandGroup<UserCommandGroup>
{
    private readonly FeedbackService _feedbackService;
    private readonly FlightTrackerService _flightTrackerService;

    public FlightCommandGroup(ICommandContext ctx,
        ILogger<UserCommandGroup> logger,
        IDiscordRestGuildAPI guildApi,
        IDiscordRestChannelAPI channelApi,
        FeedbackService feedbackService,
        FlightTrackerService flightTrackerService)
        : base(ctx, logger, guildApi, channelApi)
    {
        _feedbackService = feedbackService;
        _flightTrackerService = flightTrackerService;
    }

    [Command(nameof(TrackFlight))]
    [CommandType(ApplicationCommandType.ChatInput)]
    [Description("Track updates on for a private flight")]
    public async Task<IResult> TrackFlight([Description("Private Flight/Tail Number (e.g. N123AB)")] string input)
    {
        // TODO: https://github.com/Remora/Remora.Discord/tree/main/Remora.Discord.Interactivity

        var reply = await _feedbackService.SendContextualNeutralAsync("This feature is under development", ct: CancellationToken);

        return reply.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(reply);
    }
}