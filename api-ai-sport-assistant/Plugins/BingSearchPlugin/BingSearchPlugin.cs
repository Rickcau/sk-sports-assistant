
namespace api_ai_sport_assistant.Plugins
{
    // has a dependency on AzureDBService.cs
    using Microsoft.SemanticKernel;
    using api_ai_sport_assistant.Services;
    using System.ComponentModel;
    using Microsoft.SemanticKernel.ChatCompletion;
    using System.ComponentModel.DataAnnotations;
    using api_ai_sport_assistant.Util;

    public class BingSearchPlugin
    {
        private IBingSearchClient? _bingSearchClient;

        public BingSearchPlugin(IBingSearchClient bingSearchClient)
        {
            _bingSearchClient = bingSearchClient;
        }


        [KernelFunction]
        [Description("Performs a Bing Search to get statistics when user approves an external search.")]
        public async Task<string> GetStatisticsFromBingSearch([Description("Search external using Bing Search to get statistics about a game"), Required] string query)
        {
            Console.WriteLine($"Bing Search: {query}");

            var results = await _bingSearchClient!.SearchAsync(query);

            return results;
        }
    }
}
