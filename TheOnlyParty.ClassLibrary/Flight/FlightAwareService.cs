using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace TheOnlyParty.ClassLibrary.Flight;

public class FlightAwareService : IDisposable
{
    private readonly HttpClient _httpClient;

    public FlightAwareService(IHttpClientFactory httpClientFactory, string apiKey)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(FlightAwareService));
        _httpClient.BaseAddress = new Uri("https://aeroapi.flightaware.com/aeroapi/");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("x-apikey", apiKey);
    }

    public async Task<List<Flight>?> GetFlightsAsync(string strIdent)
    {
        FlightsResult? flightResult = null;
        var response = await _httpClient.GetAsync($"flights/{strIdent}");

        if (response.IsSuccessStatusCode)
        {
            flightResult = await response.Content.ReadFromJsonAsync<FlightsResult>();
        }

        return flightResult?.Flights;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}