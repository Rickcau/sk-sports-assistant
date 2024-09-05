
namespace web_api_ai_sport_assistant.Plugins
{
    // has a dependency on AzureDBService.cs
    using Microsoft.SemanticKernel;
    using web_api_ai_sport_assistant.Services;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class DBQueryPlugin
    {
        private static bool _hrToggleContact;
        private static string _dbConnection;

        public DBQueryPlugin(IConfiguration configuration)
        {
            _dbConnection = configuration.GetValue<string>("DatabaseConnection");
        }

        [KernelFunction]
        [Description("Executes a SQL query to get satistics about a game.")]
        public static async Task<string> GetStatistics([Description("ClientId"), Required] string clientId, string query)
        {
            Console.WriteLine($"SQL Query: {query}");

            var azureDbService = new AzureDbService(_dbConnection);
            var dbResults = await azureDbService.GetDbResults(query);

            string results = dbResults;

            Console.WriteLine($"DB Results:{results}");
            return results;
        }

        [KernelFunction]
        [Description("Executes a SQL query to all deatils about a game.")]
        public static async Task<string> GetSummary(string query)
        {
            Console.WriteLine($"SQL Query: {query}");
            var azureDbService = new AzureDbService(_dbConnection);
            var dbResults = await azureDbService.GetDbResults(query);

            string results = dbResults;

            Console.WriteLine($"DB Results:{results}");
            return results;
        }
    }
}
