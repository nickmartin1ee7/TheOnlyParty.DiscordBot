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
using Remora.Discord.Extensions.Embeds;
using System.Text.RegularExpressions;

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
    [Description("Evaluate C# Source Code using REPL")]
    public async Task<IResult> Eval([Description("Enter C# Source Code")] string code)
    {
        await LogCommandUsageAsync(nameof(Eval), code);

        if (string.IsNullOrWhiteSpace(code))
        {
            var invalidReply = await _feedbackService.SendContextualErrorAsync("Your message needs additional text.", ct: CancellationToken);
            return invalidReply.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(invalidReply);
        }

        var (isSuccess, replResult) = await _replService.Eval(code, ct: CancellationToken);

        if (!isSuccess)
        {
            var errReply = await _feedbackService.SendContextualErrorAsync("Failed to run evaluation");

            return errReply.IsSuccess
                        ? Result.FromSuccess()
                        : Result.FromError(errReply);
        }

        var status = string.IsNullOrEmpty(replResult?.Exception) ? "Success" : "Failure";

        var embed = new EmbedBuilder()
               .WithTitle($"Evaluation Result: {status}")
               .WithAuthor(_ctx.User.Username)
               .WithColour(string.IsNullOrEmpty(replResult!.Exception) ? Color.Green : Color.Red)
               .WithFooter($"Compile: {replResult!.CompileTime!.Value.TotalMilliseconds:F}ms | Execution: {replResult!.ExecutionTime!.Value.TotalMilliseconds:F}ms");

        embed.WithDescription(FormatOrEmptyCodeblock(replResult!.Code!, "cs"));

        if (replResult.ReturnValue != null)
        {
            embed.AddField($"Result: {replResult.ReturnTypeName ?? "null"}", FormatOrEmptyCodeblock($"{replResult.ReturnValue}", "json"));
        }

        if (!string.IsNullOrWhiteSpace(replResult.ConsoleOut))
        {
            embed.AddField("Console Output", FormatOrEmptyCodeblock(replResult.ConsoleOut, "txt"));
        }

        if (!string.IsNullOrWhiteSpace(replResult.Exception))
        {
            var diffFormatted = Regex.Replace($"{replResult.Exception}", "^", "- ", RegexOptions.Multiline);
            embed.AddField($"Exception: {replResult.ExceptionType}", FormatOrEmptyCodeblock(replResult.Exception!, "txt"));
        }

        var reply = await _feedbackService.SendContextualEmbedAsync(embed.Build().Entity, ct: CancellationToken);

        return reply.IsSuccess
                        ? Result.FromSuccess()
                        : Result.FromError(reply);
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

    private static string FormatOrEmptyCodeblock(string input, string prefix)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "```\n```";

        return $"```{prefix}{Environment.NewLine}{input}{Environment.NewLine}```";
    }
}