namespace web_api_ai_sport_assistant.Services
{
    using Dapper;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text.Json;
    public class AzureDbService
    {
        private readonly string _connectionString;

        public AzureDbService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<string> GetDbResults(string query)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);

            var dbResult = await connection.QueryAsync<dynamic>(query);
            var jsonString = JsonSerializer.Serialize(dbResult);

            return jsonString;
        }
    }
}

