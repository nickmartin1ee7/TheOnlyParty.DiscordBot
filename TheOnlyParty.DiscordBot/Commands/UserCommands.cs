using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using System.ComponentModel;
using System.Drawing;

using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Rest.Core;
using Remora.Results;
using TheOnlyParty.DiscordBot.Extensions;

namespace TheOnlyParty.DiscordBot.Commands;

internal class UserCommandGroup : LoggedCommandGroup<UserCommandGroup>
{
    private readonly FeedbackService _feedbackService;

    public UserCommandGroup(ICommandContext ctx,
        ILogger<UserCommandGroup> logger,
        IDiscordRestGuildAPI guildApi,
        IDiscordRestChannelAPI channelApi,
        FeedbackService feedbackService)
        : base(ctx, logger, guildApi, channelApi)
    {
        _feedbackService = feedbackService;
    }

    [Command(nameof(Eval))]

    [CommandType(ApplicationCommandType.ChatInput)]
    [Description("Leave feedback for the developer")]
    public async Task<IResult> Eval([Description("Enter your feedback to the developer")] string text)
    {
        await LogCommandUsageAsync(text);

        if (string.IsNullOrWhiteSpace(text))
        {
            var invalidReply = await _feedbackService.SendContextualErrorAsync("Your message needs additional text.");
            return invalidReply.IsSuccess
                            ? Result.FromSuccess()
                            : Result.FromError(invalidReply);
        }

        
    }

    [Command(nameof(Feedback))]
    [CommandType(ApplicationCommandType.ChatInput)]
    [Ephemeral]
    [Description("Leave feedback for the developer")]
    public async Task<IResult> Feedback([Description("Enter your feedback to the developer")] string text)
    {
        await LogCommandUsageAsync(text);

        if (string.IsNullOrWhiteSpace(text))
        {
            var invalidReply = await _feedbackService.SendContextualErrorAsync("Your feedback must contain a message.");
            return invalidReply.IsSuccess
                            ? Result.FromSuccess()
                            : Result.FromError(invalidReply);
        }

        _logger.LogInformation("New feedback left by {userName}. Feedback: {feedbackText}", _ctx.User.ToFullUsername(), text.Trim());

        var reply = await _feedbackService.SendContextualEmbedAsync(new Embed("Feedback Submitted",
                Description: "Thank you for your feedback! A developer will review your comments shortly.",
                Colour: new Optional<Color>(Color.PaleVioletRed)),
            ct: CancellationToken);

        return reply.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(reply);
    }
}