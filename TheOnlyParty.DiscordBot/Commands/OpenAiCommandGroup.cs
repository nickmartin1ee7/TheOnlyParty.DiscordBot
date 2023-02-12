using System.ComponentModel;
using System.Drawing;

using OpenAI_API;
using OpenAI_API.Completions;

using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Gateway;
using Remora.Rest.Core;
using Remora.Results;

namespace TheOnlyParty.DiscordBot.Commands
{
    public class OpenAiCommandGroup : LoggedCommandGroup<OpenAiCommandGroup>
    {
        private readonly DiscordGatewayClient _discordGatewayClient;
        private readonly FeedbackService _feedbackService;
        private readonly OpenAIAPI _openAiApi;

        public OpenAiCommandGroup(ICommandContext ctx,
            ILogger<OpenAiCommandGroup> logger,
            IDiscordRestGuildAPI guildApi,
            DiscordGatewayClient discordGatewayClient,
            IDiscordRestChannelAPI channelApi,
            FeedbackService feedbackService,
            OpenAIAPI openAiApi)
            : base(ctx, logger, guildApi, channelApi)
        {
            _discordGatewayClient = discordGatewayClient;
            _feedbackService = feedbackService;
            _openAiApi = openAiApi;
        }

        [Command(nameof(Chat))]
        [CommandType(ApplicationCommandType.ChatInput)]
        [Description("Talk to ChatGPT. Be mindful, this command uses credits (750 words = $0.02)!")]
        public async Task<IResult> Chat([Description("This is the message sent to ChatGPT.")] string prompt)
        {
            try
            {
                await LogCommandUsageAsync(nameof(Chat), prompt);

                var response = await _openAiApi.Completions.CreateCompletionAsync(new CompletionRequest(prompt));

                var responseCompletions = response is null
                    ? "Error"
                    : string.Join(Environment.NewLine, response.Completions);

                var reply = await _feedbackService.SendContextualEmbedAsync(new Embed("ChatGPT",
                        Description: response is null
                            ? "Error communicating with ChatGPT"
                            : $"{response.Model} was used to generate a response at {response.Created}.",
                        Fields: new List<EmbedField>
                        {
                            new EmbedField("Prompt", $"```txt{Environment.NewLine}{prompt}{Environment.NewLine}```"),
                            new EmbedField("Response", $"```txt{Environment.NewLine}{responseCompletions}{Environment.NewLine}```")
                        },
                        Colour: response is null
                            ? new Optional<Color>(Color.Red)
                            : new Optional<Color>(Color.Green),
                        Footer: new EmbedFooter($"Processing time: {response?.ProcessingTime}")),
                    ct: CancellationToken);


                return reply.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(reply);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error communicating with OpenAI");

                var reply = await _feedbackService.SendContextualErrorAsync(ex.Message);

                return reply.IsSuccess
                    ? Result.FromSuccess()
                    : Result.FromError(reply);
            }
        }
    }
}
