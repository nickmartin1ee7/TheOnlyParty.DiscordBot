using System.Text;

namespace TheOnlyParty.DiscordBot.Services;

public class ReplService : IDisposable
{
    private readonly string _replUri;
    private readonly HttpClient _client = new();

    public ReplService(string replUri)
    {
        _replUri = replUri;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    public async Task<(bool IsSuccess, string ResultContent)> Eval(string code, CancellationToken ct)
    {
        var content = new StringContent(code, Encoding.UTF8, "text/plain");
        var response = await _client.PostAsync($"{_replUri}/eval", content, ct);
        return (response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
    }
}
