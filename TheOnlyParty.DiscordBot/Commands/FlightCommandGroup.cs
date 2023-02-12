using System.ComponentModel;

using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Extensions.Embeds;
using Remora.Discord.Pagination.Extensions;
using Remora.Results;

using TheOnlyParty.ClassLibrary.Flight;

namespace TheOnlyParty.DiscordBot.Commands;

public class FlightCommandGroup : LoggedCommandGroup<UserCommandGroup>
{
    private readonly FeedbackService _feedbackService;
    private readonly FlightAwareService _flightAwareService;

    public FlightCommandGroup(ICommandContext ctx,
        ILogger<UserCommandGroup> logger,
        IDiscordRestGuildAPI guildApi,
        IDiscordRestChannelAPI channelApi,
        FeedbackService feedbackService,
        FlightAwareService flightAwareService)
        : base(ctx, logger, guildApi, channelApi)
    {
        _feedbackService = feedbackService;
        _flightAwareService = flightAwareService;
    }

    [Command(nameof(GetFlights))]
    [CommandType(ApplicationCommandType.ChatInput)]
    [Description("Returns the flight info status summary for a registration, ident, or fa_flight_id.")]
    public async Task<IResult> GetFlights([Description("The ident, registration, or fa_flight_id to fetch")] string ident)
    {
        var flights = await _flightAwareService.GetFlightsAsync(ident);

        if (!flights?.Any() ?? false)
        {
            var nullReply = await _feedbackService.SendContextualNeutralAsync("This feature is under development",
                ct: CancellationToken);

            return nullReply.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(nullReply);
        }

        var embeds = new List<Embed>(flights!.Count);

        for (var i = 0; i < flights.Count; i++)
        {
            var flight = flights[i];

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"{i + 1}/{flights.Count}: {flight.FaFlightId}");

            var props = flight.GetType().GetProperties();

            for (var j = 0; j < props.Length && j < 10; j++)
            {
                var prop = props[j];
                embedBuilder.AddField(prop.Name, prop.GetValue(flight)?.ToString() ?? "Unknown");
            }

            embeds.Add(embedBuilder.Build().Entity);
        }

        _ = _ctx.TryGetChannelID(out var channelId);
        _ = _ctx.TryGetUserID(out var userId);


        var reply = await _feedbackService.SendPaginatedMessageAsync(
            channelId.Value,
            userId.Value,
            embeds,
            ct: CancellationToken);

        return reply.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(reply);
    }
}
