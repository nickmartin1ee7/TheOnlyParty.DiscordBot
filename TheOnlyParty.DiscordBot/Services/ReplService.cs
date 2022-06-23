using System.Text;

using TheOnlyParty.DiscordBot.Models;

namespace TheOnlyParty.DiscordBot.Services;

public class ReplService : IDisposable
{
    private readonly string _replUri;
    private readonly HttpClient _client = new();

    public ReplService(string replUri)
    {
        if (string.IsNullOrEmpty(replUri))
        {
            throw new ArgumentNullException(nameof(replUri));
        }

        _replUri = replUri;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    public async Task<(bool IsSuccess, ReplResult? Result)> Eval(string code, CancellationToken ct)
    {
        var content = new StringContent(code, Encoding.UTF8, "text/plain");
        HttpResponseMessage? response = null;

        for (int i = 0; i < 3; i++)
        {
            try
            {
                response = await _client.PostAsync($"{_replUri}/eval", content, ct);
            }
            catch { }

            if (response != null)
            {
                break;
            }
        }

        if (response is null) return (false, null);

        return (true, ReplResult.DeserializeFrom(await response.Content.ReadAsStringAsync()));
    }
}
