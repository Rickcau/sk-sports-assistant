using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.RegularExpressions;


namespace api_ai_sport_assistant.Util
{
    internal static class Intent
    {
        // This function is a very powerful Intent helper, it allows you to detect the intent and take action accordingly which 
        // is much more efficient, reduces the number of tokens consumed and allows you to avoid unnecesscary calls to your AI search or the LLM
        // I have seen other attempts at this, but this approach is very powerful and you are letting the LLM attempt to detect the Intent 3 times
        // we then take the quorum of the 3 results and use that for the intent
        public static async Task<string> GetIntent(IChatCompletionService chat, string query)
        {
            // Keep the ChatHistory local since we only need it to detect the Intent
            ChatHistory chatHistory = new ChatHistory();
            var intent = "not_found"; // default
            chatHistory.AddSystemMessage($@"Return the intent of the user.The intent must be one of the following strings:
                    - databasegames: Use this intent to answer questions about games stored in SQL.
                    - bingsearch: Use this intent to when more information is needed about a game that does not exist in the SQL Database.
                    - not_found: Use this intent if you can't find a suitable answer
                    Do NOT include the word Intent in the intent response; only respond with the intent strings above.
            
                    [Examples for database games questions]
                    User question: How many turnovers where there?
                    Intent: databasegames
                    User question: What was the score of the game?
                    Intent: databasegames
                    User question: How many penalties did Ohio have in the game?
                    Intent: databasegames

                    [Examples for bing search questions]
                    Intent: bingsearch
                    User question: Summarize for me what happened in the game.
                    Intent: bingsearch
                    User question: How many touch downs did Brady score?
                    Intent: bingsearch
                    User question: What was the passing yards for Brady?
                    Intent: bingsearch
                    Per user query what is the Intent?
                    Intent:");

            chatHistory.AddUserMessage(query);

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                Temperature = .5,
                // ResultsPerPrompt = 3, // This is very important as it allows us to instruct the model to give us 3 results for the prompt in one call, this is very powerful
            };
            try
            {
                // Call the ChatCompletion assking for 3 rounds to attempt to identify that intent
                var result = await chat.GetChatMessageContentsAsync(
                        chatHistory,
                        executionSettings);

                string threeturnresult = string.Join(", ", result.Select(o => o.ToString()));
                // Now we use Regex and Linq to find the intent that is repeated the most
                var words = Regex.Split(threeturnresult.ToLower(), @"\W+")
                      .Where(w => w.Length >= 3)
                      .GroupBy(w => w)
                      .OrderByDescending(g => g.Count())
                      .First();
                intent = words.Key;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return intent;
        }
    }
}