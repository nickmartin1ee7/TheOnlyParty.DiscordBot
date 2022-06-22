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
using TheOnlyParty.DiscordBot.Services;

namespace TheOnlyParty.DiscordBot.Commands;

public class UserCommandGroup : LoggedCommandGroup<UserCommandGroup>
{
    private readonly FeedbackService _feedbackService;
    private readonly ReplService _replService;

    public UserCommandGroup(ICommandContext ctx,
        ILogger<UserCommandGroup> logger,
        IDiscordRestGuildAPI guildApi,
        IDiscordRestChannelAPI channelApi,
        FeedbackService feedbackService,
        ReplService replService)
        : base(ctx, logger, guildApi, channelApi)
    {
        _feedbackService = feedbackService;
        _replService = replService;
    }

    [Command(nameof(Eval))]
    [CommandType(ApplicationCommandType.ChatInput)]
    [Description("Leave feedback for the developer")]
    public async Task<IResult> Eval([Description("Enter your feedback to the developer")] string text)
    {
        await LogCommandUsageAsync(text);

        if (string.IsNullOrWhiteSpace(text))
        {
            var invalidReply = await _feedbackService.SendContextualErrorAsync("Your message needs additional text.", ct: CancellationToken);
            return invalidReply.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(invalidReply);
        }

        var (isSuccess, resultContent) = await _replService.Eval(text, ct: CancellationToken);

        if (isSuccess)
        {
            var reply = await _feedbackService.SendContextualEmbedAsync(new Embed(nameof(Eval),
                Description: resultContent,
                Author: new EmbedAuthor(_ctx.User.Username)), ct: CancellationToken);

            return reply.IsSuccess
                            ? Result.FromSuccess()
                            : Result.FromError(reply);
        }
        else
        {
            var reply = await _feedbackService.SendContextualErrorAsync(resultContent, ct: CancellationToken);

            return reply.IsSuccess
                            ? Result.FromSuccess()
                            : Result.FromError(reply);
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
            var invalidReply = await _feedbackService.SendContextualErrorAsync("Your feedback must contain a message.", ct: CancellationToken);

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