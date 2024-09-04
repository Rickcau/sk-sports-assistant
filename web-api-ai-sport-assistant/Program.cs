using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using web_api_ai_sport_assistant.Prompts;
using web_api_ai_sport_assistant.Services;
using web_api_ai_sport_assistant.Util;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true); // Add this line

var configuration = builder.Configuration;
var apiDeploymentName = configuration.GetValue<string>("ApiDeploymentName");
var apiEndpoint = configuration.GetValue<string>("ApiEndpoint");
var apiKey = configuration.GetValue<string>("ApiKey");
var bingSearchEndPoint = configuration.GetValue<string>("BingSearchApiEndPoint");
var bingSearchKey = configuration.GetValue<string>("BingSearchKey");

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddTransient<Kernel>(s =>
{
    var builder = Kernel.CreateBuilder();
    builder.AddAzureOpenAIChatCompletion(
        apiDeploymentName,
        apiEndpoint,
        apiKey);

    return builder.Build();
});

builder.Services.AddSingleton<IChatCompletionService>(sp =>
                     sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());

builder.Services.AddSingleton<IChatHistoryManager>(sp =>
{
    string systemmsg = CorePrompts.GetSystemPrompt();
    return new ChatHistoryManager(systemmsg);
});

builder.Services.AddHostedService<ChatHistoryCleanupService>();

builder.Services.AddHttpClient<IBingSearchClient, BingSearchClient>(client =>
{
    client.BaseAddress = new Uri(bingSearchEndPoint);
});

builder.Services.AddSingleton<IBingSearchClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient(nameof(IBingSearchClient));
    var apiKey = bingSearchKey;
    var endpoint = bingSearchEndPoint;
    return new BingSearchClient(httpClient, apiKey, endpoint);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();