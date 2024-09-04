using web_api_ai_sport_assistant.Models;
using System.Text.Json;

namespace web_api_ai_sport_assistant.Util;

// moving this to the BingSearchPlugin Folder
public class BingSearchClientOld
{
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly HttpClient _httpClient;

    public BingSearchClientOld(string apiKey, string endpoint)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);
    }

    public async Task<JsonDocument> SearchAsync(string query, int count = 20, int offset = 0, string market = "en-US")
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be empty.", nameof(query));

        var url = $"{_endpoint}/v7.0/search?q={Uri.EscapeDataString(query)}&count={count}&offset={offset}&mkt={market}";

        using (var response = await _httpClient.GetAsync(url))
        {
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<BingSearchResponse>(jsonResponse);
            if (searchResponse?.WebPages?.Value != null)
            {
                var theString = string.Join(" ", searchResponse.WebPages.Value
                                .Where(page => !string.IsNullOrEmpty(page.Snippet))
                                .Select(page => page.Snippet));

                // Use theString as needed, e.g., print or return it
                Console.WriteLine(theString);
            }

            return JsonDocument.Parse(jsonResponse);
        }
    }

    public async Task<JsonDocument> ImageSearchAsync(string query, int count = 10, int offset = 0, string market = "en-US")
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be empty.", nameof(query));

        var url = $"{_endpoint}/v7.0/images/search?q={Uri.EscapeDataString(query)}&count={count}&offset={offset}&mkt={market}";

        using (var response = await _httpClient.GetAsync(url))
        {
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(jsonResponse);
        }
    }

    public async Task<JsonDocument> NewsSearchAsync(string query, int count = 10, int offset = 0, string market = "en-US")
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be empty.", nameof(query));

        var url = $"{_endpoint}/v7.0/news/search?q={Uri.EscapeDataString(query)}&count={count}&offset={offset}&mkt={market}";

        using (var response = await _httpClient.GetAsync(url))
        {
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(jsonResponse);
        }
    }
}
