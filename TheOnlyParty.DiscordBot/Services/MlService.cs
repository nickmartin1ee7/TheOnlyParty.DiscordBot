using System.Text;

using TheOnlyParty.DiscordBot.Extensions;
using TheOnlyParty.DiscordBot.Models;

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

    public async Task<(bool IsSuccess, MlResult? Result)> Predict(string code, CancellationToken ct)
    {
        var content = new StringContent(code, Encoding.UTF8, "text/plain");
        HttpResponseMessage? response = null;

        for (int i = 0; i < 3; i++)
        {
            try
            {
                response = await _client.PostAsync($"{_mlUri}/predict", content, ct);
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
