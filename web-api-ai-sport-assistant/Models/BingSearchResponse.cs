namespace web_api_ai_sport_assistant.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class BingSearchResponse
{
    [JsonPropertyName("_type")]
    public string? Type { get; set; }

    [JsonPropertyName("queryContext")]
    public QueryContext? QueryContext { get; set; }

    [JsonPropertyName("webPages")]
    public WebPages? WebPages { get; set; }

    [JsonPropertyName("relatedSearches")]
    public RelatedSearches? RelatedSearches { get; set; }

    [JsonPropertyName("rankingResponse")]
    public RankingResponse? RankingResponse { get; set; }
}

public class QueryContext
{
    [JsonPropertyName("originalQuery")]
    public string? OriginalQuery { get; set; }
}

public class WebPages
{
    [JsonPropertyName("webSearchUrl")]
    public string? WebSearchUrl { get; set; }

    [JsonPropertyName("totalEstimatedMatches")]
    public int TotalEstimatedMatches { get; set; }

    [JsonPropertyName("value")]
    public List<WebPage>? Value { get; set; }
}

public class WebPage
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("displayUrl")]
    public string? DisplayUrl { get; set; }

    [JsonPropertyName("snippet")]
    public string? Snippet { get; set; }

    [JsonPropertyName("deepLinks")]
    public List<DeepLink>? DeepLinks { get; set; }
}

public class DeepLink
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("snippet")]
    public string? Snippet { get; set; }
}

public class RelatedSearches
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("value")]
    public List<RelatedSearch>? Value { get; set; }
}

public class RelatedSearch
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("displayText")]
    public string? DisplayText { get; set; }

    [JsonPropertyName("webSearchUrl")]
    public string? WebSearchUrl { get; set; }
}

public class RankingResponse
{
    [JsonPropertyName("mainline")]
    public Mainline? Mainline { get; set; }

    [JsonPropertyName("sidebar")]
    public Sidebar? Sidebar { get; set; }
}

public class Mainline
{
    [JsonPropertyName("items")]
    public List<MainlineItem> Items { get; set; }
}

public class MainlineItem
{
    [JsonPropertyName("answerType")]
    public string? AnswerType { get; set; }

    [JsonPropertyName("resultIndex")]
    public int ResultIndex { get; set; }

    [JsonPropertyName("value")]
    public MainlineItemValue? Value { get; set; }
}

public class MainlineItemValue
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public class Sidebar
{
    [JsonPropertyName("items")]
    public List<SidebarItem>? Items { get; set; }
}

public class SidebarItem
{
    [JsonPropertyName("answerType")]
    public string? AnswerType { get; set; }

    [JsonPropertyName("value")]
    public SidebarItemValue? Value { get; set; }
}

public class SidebarItemValue
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}
