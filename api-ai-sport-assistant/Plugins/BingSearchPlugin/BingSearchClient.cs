using api_ai_sport_assistant.Models;
using Azure.Search.Documents.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.SemanticKernel.Services;
using Google.Apis.CustomSearchAPI.v1.Data;

namespace api_ai_sport_assistant.Util;

public interface IBingSearchClient
{
    Task<string> SearchAsync(string query, int count = 20, int offset = 0, string market = "en-US");
    Task<JsonDocument> ImageSearchAsync(string query, int count = 10, int offset = 0, string market = "en-US");
    Task<JsonDocument> NewsSearchAsync(string query, int count = 10, int offset = 0, string market = "en-US");
    Task<(List<SearchResult> Results, string Summaries)> SearchForTopUrlsAsync(string query, int count = 20, int offset = 0, string market = "en-US");
}
public class BingSearchClient : IBingSearchClient
{
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly HttpClient _httpClient;

    public BingSearchClient(HttpClient httpClient, string apiKey, string endpoint)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);
    }
  
    public async Task<string> SearchAsync(string query, int count = 20, int offset = 0, string market = "en-US")
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be empty.", nameof(query));

        var url = $"{_endpoint}/v7.0/search?q={Uri.EscapeDataString(query)}&count={count}&offset={offset}&mkt={market}";

        var combinedSnippets = "";

        using (var response = await _httpClient.GetAsync(url))
        {
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<BingSearchResponse>(jsonResponse);
            if (searchResponse?.WebPages?.Value != null)
            {
                combinedSnippets = string.Join(" ", searchResponse.WebPages.Value
                                .Where(page => !string.IsNullOrEmpty(page.Snippet))
                                .Select(page => page.Snippet));

                // Use theString as needed, e.g., print or return it
                Console.WriteLine(combinedSnippets);
            }

            // return JsonDocument.Parse(jsonResponse);
            return combinedSnippets;
        }
    }


    public async Task<(List<SearchResult> Results, string Summaries)> SearchForTopUrlsAsync(string query, int count = 20, int offset = 0, string market = "en-US")
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be empty.", nameof(query));

        var url = $"{_endpoint}/v7.0/search?q={Uri.EscapeDataString(query)}&count={count}&offset={offset}&mkt={market}";

        var searchResults = new List<SearchResult>();

        using (var response = await _httpClient.GetAsync(url))
        {
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<BingSearchResponse>(jsonResponse);
            if (searchResponse?.WebPages?.Value != null)
            {
                foreach (var result in searchResponse?.WebPages?.Value!)
                {
                    searchResults.Add(new SearchResult
                    {
                        Id = result.Id,
                        Name = result.Name,
                        Url = result.Url,
                        Snippet = result.Snippet,
                        // No RankingScore, so we focus on other properties or simply return the order as-is.
                    });
                }
            }

            var topTenPages = searchResults.Take(10).ToList();
            var allSummaries = "";
            // Could have executed this logic in the foreach above, but I am doing here for now... :)
            foreach (var topTenPage in topTenPages)
            {
                var pageContent = await GetPageContentAsync(topTenPage.Url!);
                var summary = await SummarizeContentAsync(pageContent);

                if (!string.IsNullOrEmpty(summary))
                {
                    allSummaries += summary + "\n";
                }
            }

            return (searchResults, allSummaries);
        }
    }

    private async Task<string> GetPageContentAsync(string url)
    {
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(url);

        var pageContent = doc.DocumentNode.SelectSingleNode("//body").InnerText;
        return pageContent;
    }

    private async Task<string> SummarizeContentAsync(string content)
    {
        // Use AI to summarize content here
        // var aiService = new AIService(); // Replace with your AI service integration
        //var summary = await aiService.SummarizeAsync(content);
        await Task.Delay(1000);
        return "this is the summary";
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
