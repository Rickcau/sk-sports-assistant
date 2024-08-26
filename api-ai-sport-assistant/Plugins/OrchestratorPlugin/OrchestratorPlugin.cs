using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using TourGuideAgentV2.Plugins.TripPlannerPrompts;
using TourGuideAgentV2.Services;

namespace api_ai_sport_assistant.Plugins.Orchestrator
{
    public class OrchestratorPlugin
    {
        private readonly ILogger<OrchestratorPlugin> _logger;
        private readonly Kernel _kernel;

        private IChatHistoryManager _chatHistoryManager;
        private FunctionCallContentBuilder? _functionCallContentBuilder;

        private readonly ChatHistory? _lastChatHistory;

        public ChatHistory? GetLastChatHistory { get { return _lastChatHistory; } }

        public bool FunctionsToCall { get 
            { 
                var GetFunctionsToCall = this.GetFunctionsToCall();
                if (GetFunctionsToCall.Count >  0) 
                { 
                    return true; 
                } 
                else 
                { 
                    return false; 
                }           
            } 
        }
        
        public OrchestratorPlugin(
            Kernel kernel,
            IChatHistoryManager chathistorymanager,
            ILogger<OrchestratorPlugin> logger // Inject the logger here
        )
        {
            _kernel = kernel;
            _chatHistoryManager = chathistorymanager;
            _logger = logger; // Assign the logger to the readonly field
        }

        // Other methods and properties can use _logger for logging

        [KernelFunction, Description("Provides travel recommendations and information based on user queries for a vehicleId.")]
        public async IAsyncEnumerable<string> MessageLoopStreamAsync(
            [Description("message")] string message,
            [Description("clientId")] string clientId,
            [Description("vehicleId")] string vehicleId)
        {
            // Note how we had to change the return type to IAsyncEnumerable<string> and use the yield return statement
            _logger.LogInformation("Inside AgentPlugin's ChatStreamAsync");
             // We need a new instance of FunctionCallContentBuilder to build the list of function calls.
            _functionCallContentBuilder = new FunctionCallContentBuilder();
            var chatHistory = _chatHistoryManager.GetOrCreateChatHistory(clientId);
            // Use the chatHistory for this specific client
            chatHistory.AddUserMessage(message);
            // Person is already in the Program.cs for the IChatHistoryManager so whcn GetOrCreateChatHistory is called the system message is already added 
            chatHistory.AddUserMessage($"Request:{message}"); // setting user's ask using chatHistory for the clientID allows us to persist the chat history for the client across multiple requests
            // the ChatHistoryManager will keep track of the chat history for each client for 1 hour then the background service will clean it up
            // this allows the client to have a conversation with the agent over multiple requests and the agent can remember the context of the conversation for 1 hr.
            // the chat history is stored in memory so if the server restarts the chat history will be lost.
            // this approach could be replaced by storing the chat history in a database or other persistent storage if needed.

            var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();

            // configuring behavior of chat completion service
            var executionSettings = new OpenAIPromptExecutionSettings
            {

                ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions
                // ResponseFormat = "json_object"
            };
            var assistantResponse = "";
            await foreach (var chatUpdate in chatCompletion.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings, _kernel))
            {

                if (chatUpdate!=null ) 
                {
                   // Use the FunctionCallContentBuilder to build a list of function calls and append them as we stream the response back to the client.
                   if (chatUpdate.Role == AuthorRole.Assistant)
                   {
                       assistantResponse += chatUpdate.Content;
                   }
                //    else if (chatUpdate.Role == AuthorRole.Tool)
                //    {
                //       _functionCallContentBuilder.Append(chatUpdate); 
                //    } 
                   _functionCallContentBuilder.Append(chatUpdate);      
                   yield return chatUpdate!.ToString();
                }
            }   
            // localChatHistory.AddAssistantMessage(assistantResponse); // we may need this in the future..setting assistant's response in local chat history 

        }

        // This is used to allow the 
        public IReadOnlyList<FunctionCallContent> GetFunctionsToCall()
        {
            // Here is an example of invoking all the function calls in parallel. using tasks.
            var functionCalls = _functionCallContentBuilder?.Build();
            return functionCalls!;
        }

        // We need to call the function here TBD
        private bool CallFunction(string vehicleId, string message, string jobId, string type)
        {
            
            return true;
        }
        
    }
}
