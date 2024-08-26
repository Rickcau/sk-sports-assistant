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
        private readonly IBingSearchClient _bingSearchClient;
        // private readonly ChatHistory _chatHistory;

        public ChatProvider(ILogger<ChatProvider> logger, Kernel kernel, IChatCompletionService chat, IChatHistoryManager chathistorymanager, IBingSearchClient bingSearchClient)
        {
            _logger = logger;
            _kernel = kernel;
            _chat = chat;
            _chatHistoryManager = chathistorymanager;
            _bingSearchClient = bingSearchClient;
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
            _kernel.ImportPluginFromObject( new BingSearchPlugin(_bingSearchClient)); 

            var response = new ChatProviderResponse();

            var intent = chatRequest.type;

            //var dbSchema = string.Empty;
            var jsonSchema = string.Empty;
            string[]? tableNames = null;

            tableNames = "dbo.FootballGames".Split(","); // You can have more that one table defined here
            jsonSchema = await sqlHarness.ReverseEngineerSchemaJSONAsync(tableNames);

            chatHistory.AddUserMessage(NLPSqlPluginPrompts.GetNLPToSQLSystemPrompt(jsonSchema));
            chatHistory.AddUserMessage(chatRequest.prompt);

            ChatMessageContent? result = null;

            result = await _chat.GetChatMessageContentAsync(
                  chatHistory,
                  executionSettings: new OpenAIPromptExecutionSettings { Temperature = 0.8, TopP = 0.0, ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions },
                  kernel: _kernel);

            Console.WriteLine(result.Content);

            response.ChatResponse = result.Content;

            return new OkObjectResult(response);

        }
    }
}
