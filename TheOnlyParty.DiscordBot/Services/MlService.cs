using System.Text;
using System.Text.Json;

using TheOnlyParty.ClassLibrary;
using TheOnlyParty.DiscordBot.Extensions;

namespace TheOnlyParty.DiscordBot.Services;

public class MlService : IDisposable
{
    private readonly string _mlUri;
    private readonly HttpClient _client = new();

    public MlService(string mlUri)
    {
        if (string.IsNullOrEmpty(mlUri))
        {
            throw new ArgumentNullException(nameof(mlUri));
        }

        _mlUri = mlUri;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    public async Task<(bool IsSuccess, MlResult? Result)> PredictAsync(string input, CancellationToken ct = default)
    {
        HttpResponseMessage? response = null;

        for (int i = 0; i < 3; i++)
        {
            try
            {
                response = await _client.PostAsync($"{_mlUri}/predict",
                    new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json"),
                    ct);
            }
            catch { }

            if (response != null)
            {
                break;
            }
        }

        if (response is null) return (false, null);

        return (true, (await response.Content.ReadAsStringAsync(ct)).FromJson<MlResult>());
    }
}
