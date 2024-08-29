using static System.Net.Mime.MediaTypeNames;
using System.Text.Json.Serialization;



namespace api_ai_sport_assistant.Models;
public class SearchResult
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("snippet")]
    public string? Snippet { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}