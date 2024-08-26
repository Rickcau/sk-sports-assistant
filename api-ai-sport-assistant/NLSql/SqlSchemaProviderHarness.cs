// Copyright (c) Microsoft. All rights reserved.
using System.Data.Common;
using System.Data.SqlClient;
using SemanticKernel.Data.Nl2Sql.Library.Schema;

namespace SemanticKernel.Data.Nl2Sql.Harness;

/// <summary>
/// Harness for utilizing <see cref="SqlSchemaProvider"/> to capture live database schema
/// definitions: <see cref="SchemaDefinition"/>.
/// </summary>
public sealed class SqlSchemaProviderHarness
{
    public SqlSchemaProviderHarness()
    {

    }

    private static string _dbConnection = Environment.GetEnvironmentVariable("DatabaseConnection", EnvironmentVariableTarget.Process) ?? string.Empty;
    private static string _dbDescription = Environment.GetEnvironmentVariable("DatabaseDescription", EnvironmentVariableTarget.Process) ?? string.Empty;

    public async Task<string> ReverseEngineerSchemaYAMLAsync(string[] tableNames)
    {
        string dbName = GetDatabaseName();
        var yaml = await this.CaptureSchemaYAMLAsync(dbName, _dbConnection, _dbDescription, tableNames).ConfigureAwait(false);

        return yaml;    
    }

    public async Task<string> ReverseEngineerSchemaJSONAsync(string[] tableNames)
    {
        string dbName = GetDatabaseName();
        var yaml = await this.CaptureSchemaJSONAsync(dbName, _dbConnection, _dbDescription, tableNames).ConfigureAwait(false);

        return yaml;
    }

    private async Task<string> CaptureSchemaYAMLAsync(string databaseKey, string? connectionString, string? description, params string[] tableNames)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        var provider = new SqlSchemaProvider(connection);

        var schema = await provider.GetSchemaAsync(databaseKey, description, tableNames).ConfigureAwait(false);

        await connection.CloseAsync().ConfigureAwait(false);

        var yamlText = await schema.FormatAsync(YamlSchemaFormatter.Instance).ConfigureAwait(false);
        
        return yamlText;

        // If you want to save to a file    
        //await this.SaveSchemaAsync("yaml", databaseKey, yamlText).ConfigureAwait(false);
        //await this.SaveSchemaAsync("json", databaseKey, schema.ToJson()).ConfigureAwait(false);
    }

    private async Task<string> CaptureSchemaJSONAsync(string databaseKey, string? connectionString, string? description, params string[] tableNames)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        var provider = new SqlSchemaProvider(connection);

        var schema = await provider.GetSchemaAsync(databaseKey, description, tableNames).ConfigureAwait(false);

        await connection.CloseAsync().ConfigureAwait(false);

        var yamlText = await schema.FormatAsync(YamlSchemaFormatter.Instance).ConfigureAwait(false);

        return schema.ToJson();

        // If you want to save to a file
        //await this.SaveSchemaAsync("yaml", databaseKey, yamlText).ConfigureAwait(false);
        //await this.SaveSchemaAsync("json", databaseKey, schema.ToJson()).ConfigureAwait(false);
    }

    private static string GetDatabaseName()
    {
        DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
        builder.ConnectionString = _dbConnection;
        object databaseName;

        if (builder.TryGetValue("Initial Catalog", out databaseName) || builder.TryGetValue("Database", out databaseName))
        {
            return databaseName.ToString();
        }
        else
        {
            return string.Empty;
        }
    }
}