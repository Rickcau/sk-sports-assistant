using api_ai_sport_assistant.NLPSqlPrompts;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.RegularExpressions;
using api_ai_sport_assistant.Models;
using System.Text.Json;
using Azure;


namespace api_ai_sport_assistant.Util
{
    internal static class BingSearchHelper
    {
        // This function is a very powerful Intent helper, it allows you to detect the intent and take action accordingly which 
        // is much more efficient, reduces the number of tokens consumed and allows you to avoid unnecesscary calls to your AI search or the LLM
        // I have seen other attempts at this, but this approach is very powerful and you are letting the LLM attempt to detect the Intent 3 times
        // we then take the quorum of the 3 results and use that for the intent
        public static async Task<BingSearchTopic> GetSearchActions(IChatCompletionService chat, string query)
        {
            // Keep the ChatHistory local since we only need it to detect the Intent
            ChatHistory chatHistory = new ChatHistory();
            var extractSearchActions = BingSearchPrompts.GetBingActionPrompt(query);
            chatHistory.AddUserMessage(extractSearchActions);

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                Temperature = .5,
                ResponseFormat = "json_object"
                // ResultsPerPrompt = 3, // This is very important as it allows us to instruct the model to give us 3 results for the prompt in one call, this is very powerful
            };
            BingSearchTopic? bingSearchTopicActions =  null;
            try
            {
                // Call the ChatCompletion assking for 3 rounds to attempt to identify that intent
                var result = await chat.GetChatMessageContentsAsync(
                        chatHistory,
                        executionSettings);
                bingSearchTopicActions = JsonSerializer.Deserialize<BingSearchTopic>(result[0].ToString() ?? "");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return bingSearchTopicActions;
        }
    }
}