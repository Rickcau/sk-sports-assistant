
using api_ai_sport_assistant.Interfaces;
using api_ai_sport_assistant.Plugins;
using api_ai_sport_assistant.Services;
using api_ai_sport_assistant.Util;
using Azure.Search.Documents.Indexes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using api_ai_sport_assistant.CorePrompts;

//
string ApiDeploymentName = Environment.GetEnvironmentVariable("ApiDeploymentName", EnvironmentVariableTarget.Process) ?? "";
string ApiEndpoint = Environment.GetEnvironmentVariable("ApiEndpoint", EnvironmentVariableTarget.Process) ?? "";
string ApiKey = Environment.GetEnvironmentVariable("ApiKey", EnvironmentVariableTarget.Process) ?? "";
string TextEmbeddingName = Environment.GetEnvironmentVariable("EmbeddingName", EnvironmentVariableTarget.Process) ?? "";
string BingSearchEndPoint = Environment.GetEnvironmentVariable("BingSearchApiEndPoint", EnvironmentVariableTarget.Process) ?? "";
string BingSearchKey = Environment.GetEnvironmentVariable("BingSearchKey", EnvironmentVariableTarget.Process) ?? "";

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddTransient<Kernel>(s =>
        {
            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion(
                ApiDeploymentName,
                ApiEndpoint,
                ApiKey
                );

            return builder.Build();
        });

        services.AddSingleton<IChatCompletionService>(sp =>
                     sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());
        // this is not needed as we are using the ChatHistoryManager below which is an in-memory cache with that TTL of 1hr
        // you can adjust the TTL in the ChatHistoryCleanUpService
        // this is very effective for maintaining context over sort duration, but it long-term persistence is needed, have to store/retreive from CosmosDB.
        
        //services.AddSingleton<ChatHistory>(s =>
        //{
        //    var chathistory = new ChatHistory();
        //    return chathistory;
        //});

        // Add the ChatHistoryManager as a singleton service to manage chat histories based on client ID
        services.AddSingleton<IChatHistoryManager>(sp =>
        {
            string systemmsg = CorePrompts.GetSystemPrompt();
            return new ChatHistoryManager(systemmsg);
        });

        // AddHostedService - ASP.NET will run the ChatHistoryCleanupService in the background and will clean up all chathistores that are older than 1 hour
        services.AddHostedService<ChatHistoryCleanupService>();

        services.AddHttpClient<IBingSearchClient, BingSearchClient>(client =>
        {
            client.BaseAddress = new Uri(BingSearchEndPoint);
        });

        services.AddSingleton<IBingSearchClient>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(IBingSearchClient));
            var apiKey = BingSearchKey;
            var endpoint = BingSearchEndPoint;
            return new BingSearchClient(httpClient, apiKey, endpoint);
        });

    })
    .Build();

host.Run();

