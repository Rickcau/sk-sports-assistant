// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using SemanticKernel.Data.Nl2Sql.Library.Internal;
using SemanticKernel.Data.Nl2Sql.Library.Schema;

namespace SemanticKernel.Data.Nl2Sql.Library;

/// <summary>
/// Generate SQL query targeting Microsoft SQL Server.
/// </summary>
public sealed class SqlQueryGenerator
{
    public const double DefaultMinRelevance = 0.7D;

    public const string ContextParamObjective = "data_objective";
    public const string ContextParamSchema = "data_schema";
    public const string ContextParamSchemaId = "data_schema_id";
    public const string ContextParamQuery = "data_query";
    public const string ContextParamPlatform = "data_platform";
    public const string ContextParamError = "data_error";
    public const string ContextParamRules = "data_rules";

    private const string ContentLabelQuery = "sql";
    private const string ContentLabelAnswer = "answer";
    private const string ContentAffirmative = "yes";

    private readonly double _minRelevanceScore;
    private readonly KernelFunction _promptEval;
    private readonly KernelFunction _promptGenerator;
    private readonly Kernel _kernel;
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly ISemanticTextMemory _memory;
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    public SqlQueryGenerator(
        Kernel kernel,
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        ISemanticTextMemory memory,
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        string isQueryText,
        string generateQueryText,
        double minRelevanceScore = DefaultMinRelevance)
    {
        this._promptEval = KernelFunctionFactory.CreateFromPrompt(isQueryText);
        this._promptGenerator = KernelFunctionFactory.CreateFromPrompt(generateQueryText);
        this._kernel = kernel;
        this._memory = memory;
        this._minRelevanceScore = minRelevanceScore;
    }

    /// <summary>
    /// Attempt to produce a query for the given objective based on the registered schemas.
    /// </summary>
    /// <param name="objective">A natural language objective</param>
    /// <param name="context">A <see cref="SKContext"/> object</param>
    /// <returns>A SQL query (or null if not able)</returns>
    public async Task<SqlQueryResult?> SolveObjectiveAsync(string objective)
    {
        // Search for schema with best similarity match to the objective
        var recall =
            await this._memory.SearchAsync(
                SchemaProvider.MemoryCollectionName,
                objective,
                limit: 1, // Take top result with maximum relevance (> minRelevanceScore)
                minRelevanceScore: this._minRelevanceScore,
                withEmbeddings: true).ToArrayAsync().ConfigureAwait(false);

        var best = recall.FirstOrDefault();
        if (best == null)
        {
            return null; // No schema / no query
        }

        var schemaName = best.Metadata.Id;
        var schemaText = best.Metadata.Text;
        var sqlPlatform = best.Metadata.AdditionalMetadata;
        var rules = best.Metadata.Description;
        var arguments = new KernelArguments();
        arguments[ContextParamObjective] = objective;
        arguments[ContextParamSchema] = schemaText;
        arguments[ContextParamSchemaId] = schemaName;
        arguments[ContextParamPlatform] = sqlPlatform;
        arguments[ContextParamRules] = rules;
        // Screen objective to determine if it can be solved with the selected schema.
        if (!await this.ScreenObjectiveAsync(arguments).ConfigureAwait(false))
        {
            //return null; // Objective doesn't pass screen
        }

        // Generate query
        var result = await this._promptGenerator.InvokeAsync(this._kernel, arguments).ConfigureAwait(false);

        // Parse result to handle 
        string query = result.ParseValue(ContentLabelQuery);

        return new SqlQueryResult(schemaName, query);
    }

    private async Task<bool> ScreenObjectiveAsync(KernelArguments context)
    {
        var result = await this._promptEval.InvokeAsync(this._kernel, context).ConfigureAwait(false);

        var answer = result.ParseValue(ContentLabelAnswer);
        return answer.Equals(ContentAffirmative, StringComparison.OrdinalIgnoreCase);
    }
}
