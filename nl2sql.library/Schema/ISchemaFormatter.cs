// Copyright (c) Microsoft. All rights reserved.

using System.IO;
using System.Threading.Tasks;

namespace SemanticKernel.Data.Nl2Sql.Library.Schema;

public interface ISchemaFormatter
{
    Task WriteAsync(TextWriter writer, SchemaDefinition schema);
}
