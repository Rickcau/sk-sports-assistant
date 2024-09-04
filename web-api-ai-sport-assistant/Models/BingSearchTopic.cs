using System.Text.Json.Serialization;

namespace web_api_ai_sport_assistant.Models;

public class BingSearchTopic
{
    [JsonPropertyName("topic")]
    public string? Topic { get; set; }

    [JsonPropertyName("data")]
    public List<ActionItem>? Data { get; set; }
}

public class ActionItem
{
    [JsonPropertyName("action")]
    public string? Action { get; set; }
}
