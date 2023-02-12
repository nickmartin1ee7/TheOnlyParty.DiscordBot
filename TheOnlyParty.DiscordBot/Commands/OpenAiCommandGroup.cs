using System.ComponentModel;
using System.Drawing;

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

        public OpenAiCommandGroup(ICommandContext ctx,
            ILogger<OpenAiCommandGroup> logger,
            IDiscordRestGuildAPI guildApi,
            DiscordGatewayClient discordGatewayClient,
            IDiscordRestChannelAPI channelApi,
            FeedbackService feedbackService)
            : base(ctx, logger, guildApi, channelApi)
        {
            _discordGatewayClient = discordGatewayClient;
            _feedbackService = feedbackService;
        }

        [Command(nameof(Chat))]
        [CommandType(ApplicationCommandType.ChatInput)]
        [Description("Talk to ChatGPT. Be mindful, this command uses credits (750 words = $0.02)!")]
        public async Task<IResult> Chat([Description("This is the message sent to ChatGPT.")] string prompt)
        {
            try
            {
                await LogCommandUsageAsync(nameof(Chat), prompt);


                var reply = await _feedbackService.SendContextualEmbedAsync(new Embed("ChatGPT",
                        Description: responseStatus,
                        Fields: new List<EmbedField>
                        {
                            new EmbedField("Prompt", $"```txt{Environment.NewLine}{prompt}{Environment.NewLine}```"),
                            new EmbedField("Response", $"```txt{Environment.NewLine}{response}{Environment.NewLine}```")
                        },
                        Colour: new Optional<Color>(Color.Red),
                        Footer: new EmbedFooter($"Elapsed time: {responseTime}")),
                    ct: CancellationToken);


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
