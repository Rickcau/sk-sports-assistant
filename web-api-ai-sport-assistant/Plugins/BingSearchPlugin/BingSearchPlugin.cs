
namespace web_api_ai_sport_assistant.Plugins
{
    // has a dependency on AzureDBService.cs
    using Microsoft.SemanticKernel;
    using web_api_ai_sport_assistant.Services;
    using System.ComponentModel;
    using Microsoft.SemanticKernel.ChatCompletion;
    using System.ComponentModel.DataAnnotations;
    using web_api_ai_sport_assistant.Util;
    using web_api_ai_sport_assistant.Models;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    public class BingSearchPlugin
    {
        private IBingSearchClient? _bingSearchClient;
        //private readonly IChatCompletionService _chat;
        public BingSearchPlugin(IBingSearchClient bingSearchClient)
        {
            _bingSearchClient = bingSearchClient;
           // _chat = chat;   
        }


        [KernelFunction]
        [Description("Performs a Bing Search to get statistics when user approves an external search.")]
        public async Task<string> GetStatisticsFromBingSearch([Description("Search external using Bing Search to get statistics about a game"), Required] string query, [Description("Common separated list of stats to retreive"), Required] string actions)
        {
            Console.WriteLine($"Bing Search: {query} {actions}");

            var searchQuery = $"{query} {actions}";

            var test = $"Provide a summary of the 11/2/23 NCAA Football -Ohio St vs Michigan college football game include the number of penalties and reviews";
            var results = await _bingSearchClient!.SearchAsync(searchQuery);

            //var test = "Provide a summary of the 11.2.23 NCAA Football - Ohio St vs Michigan college football game include the number of penalties and reviews.";

            //BingSearchTopic? bingSearchTopicActionResults = await BingSearchHelper.GetSearchActions(_chat, test);

            //// NOt sure this loop will me needed, but it might make sense, more testing is needed.
            //foreach (var actionItem in bingSearchTopicActionResults.Data)
            //{
            //    var snippetresult = await _bingSearchClient!.SearchAsync($"Site: espn.com or ncaa.com Topic: {bingSearchTopicActionResults.Topic} get: {actionItem.Action}");
            //    snippets.Add(snippetresult.ToString());
            //    Console.WriteLine($"Action: {actionItem.Action}");
            //}

            //// Build a common separated list of actions needed


            // var snippetresult = await _bingSearchClient!.SearchAsync($"Site: espn.com or ncaa.com Topic: {bingSearchTopicActionResults.Topic} get: {actionItem.Action}");


            return results;
        }
    }
}
