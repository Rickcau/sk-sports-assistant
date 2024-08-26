// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Memory;

namespace SemanticKernel.Data.Nl2Sql.Library.Schema;

/// <summary>
/// Responsible for loading the defined schemas into kernel memory.
/// </summary>
public static class SchemaProvider
{
    public const string MemoryCollectionName = "data-schemas";

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public static async Task InitializeAsync(ISemanticTextMemory memory, IEnumerable<string> schemaPaths)
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    {
        foreach (var schemaPath in schemaPaths)
        {
            var schema = await SchemaSerializer.ReadAsync($"{schemaPath}.json").ConfigureAwait(false);
            var descFile = $"{schemaPath}.md";
            string? desc = null;
            if (File.Exists(descFile))
            {
                desc = await File.ReadAllTextAsync(descFile).ConfigureAwait(false);
            }
            var schemaText = await schema.FormatAsync(YamlSchemaFormatter.Instance).ConfigureAwait(false);

            await memory.SaveInformationAsync(MemoryCollectionName, schemaText, schema.Name, desc, additionalMetadata: schema.Platform).ConfigureAwait(false);
        }
    }
}
