using api_ai_sport_assistant.Models;
using api_ai_sport_assistant.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernel.Data.Nl2Sql.Harness;
using api_ai_sport_assistant.NLPSqlPrompts;
using System.Net;
using System.Text.Json;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;
using api_ai_sport_assistant.Services;
using api_ai_sport_assistant.Plugins;

namespace api_ai_sport_assistant.Functions
{
    public class ChatProvider
    {
        private readonly ILogger<ChatProvider> _logger;
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chat;
        private readonly IChatHistoryManager _chatHistoryManager;
        // private readonly ChatHistory _chatHistory;

        public ChatProvider(ILogger<ChatProvider> logger, Kernel kernel, IChatCompletionService chat, IChatHistoryManager chathistorymanager)
        {
            _logger = logger;
            _kernel = kernel;
            _chat = chat;
            _chatHistoryManager = chathistorymanager;
        }

        [Function("ChatProvider")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            // Request body example:
            /*
                {
                    "userId": "stevesmith@contoso.com",
                    "sessionId": "12345678",
                    "tenantId": "00001",
                    "type": "internal", // "external"
                    "chatName": "New Chat",
                    "prompt": "Hello, What can you do for me?"
                }
            */

            _logger.LogInformation("C# ChatProvider HTTP Post trigger function processed a request.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var chatRequest = JsonSerializer.Deserialize<ChatProviderRequest>(requestBody);
            if (chatRequest == null || chatRequest.userId == null || chatRequest.sessionId == null || chatRequest.tenantId == null || chatRequest.prompt == null)
            {
                throw new ArgumentNullException("Please check your request body, you are missing required data.");
            }

            if (string.IsNullOrEmpty(chatRequest.sessionId))
            {
                // needed for new chats
                chatRequest.sessionId = Guid.NewGuid().ToString();
            }
            var sessionId = chatRequest.sessionId;
            var chatHistory = _chatHistoryManager.GetOrCreateChatHistory(sessionId);

            var sqlHarness = new SqlSchemaProviderHarness();

            // _kernel.ImportPluginFromObject(new TripPlannerPlugin(_chat, _chatHistoryManager, _jobResultsCacheService, _kernel), "TripPlannerPlugin");

            _kernel.ImportPluginFromObject( new DBQueryPlugin());

            var response = new ChatProviderResponse();

            var intent = chatRequest.type;

            bool databaseIntent = false;

            //var dbSchema = string.Empty;
            var jsonSchema = string.Empty;
            string[]? tableNames = null;
            // The purpose of using an Intent pattern is to allow you to make decisions about how you want to invoke the LLM
            // In the case of RAG, if you can detect what the user intent is to related to, then you can only perform that action
            // this allows you to reduce the token useage and save you TPM and cost
            switch (intent)
            {
                case "internal":  // When intent is internal we search the SQL Tables
                    {
                        databaseIntent = true;

                        // At this point we know the intent is database related so we could just call the plugin
                        // directly, but since we have AutoInvokeKernelFunctions enabled,
                        // we can just let SK detect that it needs to call the function and let it do it. However,
                        // it would be more performant to just call it directly as there is additional overhead
                        // with SK searching the plugin collection.
                        Console.WriteLine("Intent: databaseproduct");

                        tableNames = "dbo.FootballGames".Split(","); // You can have more that one table defined here
                        jsonSchema = await sqlHarness.ReverseEngineerSchemaJSONAsync(tableNames);
                        _chatHistory.AddSystemMessage(NLPSqlPluginPrompts.GetNLPToSQLSystemPrompt(jsonSchema));
                        _chatHistory.AddUserMessage(chatRequest.prompt);

                        break;
                    }
                case "bingsearch":
                    {
                        // Now that I know the intent of the question is graphql related, I could just call the plugin directly
                        // but, since I have AutoInvokeKernelFunctions enabled I can just let SK detect that it needs to call the funciton and let it do it.
                        // Now, it would be more performant to just call it directly as their is additional overhead with SK searching the plugin collection etc

                        // Let's extract the Topic and the actions needed for the search so we can get better results

                        var test = "Provide a summary of the 11.2.23 NCAA Football - Ohio St vs Michigan college football game include the number of penalties and reviews.";

                        BingSearchTopic? bingSearchTopicActionResults = await BingSearchHelper.GetSearchActions(_chat, test);

                        // Next we need to get results for each of the actions individually via multiple calls to Bing Search so we have snippets for each of the actions
                        // being requested, then we have the LLM summarize these results.

                        // TBD
                        List<string> snippets = new List<string>();

                        if (bingSearchTopicActionResults != null && bingSearchTopicActionResults?.Data != null)
                        {
                            BingSearchClient bingClient = new BingSearchClient("cb54243792e6463580096373a798e996", "https://api.bing.microsoft.com/");
                            var topic = bingSearchTopicActionResults.Topic;
                            foreach (var actionItem in bingSearchTopicActionResults.Data)
                            {
                                var snippetresult = await bingClient.SearchAsync($"Site: espn.com or ncaa.com Topic: {topic} get: {actionItem.Action}");
                                snippets.Add(snippetresult.ToString());
                                Console.WriteLine($"Action: {actionItem.Action}");
                            }
                        }

                        // RDC - Had issues with the Bing Connector so I wrote my own BingClient Helper
                        // var myresult = await bingClient.SearchAsync("Provide a summary of the [11.2.23 NCAA Football - Ohio St vs Michigan] college football game include the number of penalties and reviews.");
                        // Console.WriteLine(myresult.ToString());
                        //var x = await plugin.SearchAsync("What is Semantic Kernel from Microsoft?");
//#pragma warning disable SKEXP0050
//                        var bingConnector = new BingConnector("123456", new Uri("https://api.bing.microsoft.com/"));
//                        var plugin = new WebSearchEnginePlugin(bingConnector);

//                        var x = await plugin.SearchAsync("What is Semantic Kernel from Microsoft?");
//                        _kernel.ImportPluginFromObject(plugin, "BingPlugin");
//                        OpenAIPromptExecutionSettings settings = new()
//                        {
//                            ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions,
//                        };
//                        var chatHistory = new ChatHistory();
//                        chatHistory.AddMessage(AuthorRole.User, "What is Semantic Kernel from Microsoft?");

//                        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
//                        var result2 = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, _kernel);

//                        var functionCalls = ((OpenAIChatMessageContent)result2).GetOpenAIFunctionToolCalls();
//                        foreach (var functionCall in functionCalls)
//                        {
//                            KernelFunction pluginFunction;
//                            KernelArguments arguments;
//                            _kernel.Plugins.TryGetFunctionAndArguments(functionCall, out pluginFunction, out arguments);
//                            var functionResult = await _kernel.InvokeAsync(pluginFunction!, arguments!);
//                            var jsonResponse = functionResult.GetValue<object>();
//                            var json = JsonSerializer.Serialize(jsonResponse);
//                            Console.WriteLine(json);
//                            chatHistory.AddMessage(AuthorRole.Tool, json);
//                        }


#pragma warning disable SKEXP0050

                        Console.WriteLine("Intent: bingsearch");
                        break;
                    }
                case "not_found":
                    {
                        _chatHistory.AddSystemMessage(NLPSqlPluginPrompts.GetNotFoundSystemPrompt);
                        Console.WriteLine("Intent: not_found");
                        break;
                    }

            }
            ChatMessageContent? result = null;

            // TBD: the code below would accompany the code commented out at the top of this function
            // It's purpose is to insert the user message into the session within cosmosDB
            /******** Create message for this session in cosmos DB ********/
            //var message = new Message()
            //{
            //    Id = Guid.NewGuid().ToString(),

            //    Type = "message",
            //    Sender = "user",
            //    SessionId = chatRequest.sessionId,
            //    TimeStamp = DateTime.UtcNow,
            //    Prompt = chatRequest.prompt,
            //};

            //// Insert user prompt
            //await _azureCosmosDbService.InsertMessageAsync(message);
            /******** Create chat session in cosmos DB ********/

            result = await _chat.GetChatMessageContentAsync(
                  _chatHistory,
                  executionSettings: new OpenAIPromptExecutionSettings { Temperature = 0.8, TopP = 0.0, ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions },
                  kernel: _kernel);
            
            Console.WriteLine(result.Content);

            // TBD: the code below would accompany the code commented out at the top of this function
            // It's purpose is to insert the system message into the session within cosmosDB
            // insert systems response
            //message = new Message()
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Type = "message",
            //    Sender = "system",
            //    SessionId = chatRequest.sessionId,
            //    TimeStamp = DateTime.UtcNow,
            //    Prompt = result.Content,
            //};

            //await _azureCosmosDbService.InsertMessageAsync(message);

            response.ChatResponse = result.Content;

            return new OkObjectResult(response);

        }
    }
}
