using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using web_api_ai_sport_assistant.Services;
using web_api_ai_sport_assistant.Util;
using web_api_ai_sport_assistant.Models;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernel.Data.Nl2Sql.Harness;
using web_api_ai_sport_assistant.Plugins;
using web_api_ai_sport_assistant.NLPSqlPrompts;

namespace web_api_ai_sport_assistant.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chat;
        private readonly IChatHistoryManager _chatHistoryManager;
        private readonly IBingSearchClient _bingSearchClient;
        private readonly IConfiguration _configuration;

        public ChatController(
            ILogger<ChatController> logger, 
            Kernel kernel, 
            IChatCompletionService chat, 
            IChatHistoryManager chathistorymanager, 
            IBingSearchClient bingSearchClient,
            IConfiguration configuration)
        {
            _logger = logger;
            _kernel = kernel;
            _chat = chat;
            _chatHistoryManager = chathistorymanager;
            _bingSearchClient = bingSearchClient;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatProviderRequest chatRequest)
        {
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

            var sqlHarness = new SqlSchemaProviderHarness(_configuration);

            _kernel.ImportPluginFromObject(new DBQueryPlugin(_configuration));
            _kernel.ImportPluginFromObject(new BingSearchPlugin(_bingSearchClient));

            var response = new ChatProviderResponse();

            var intent = chatRequest.type;

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