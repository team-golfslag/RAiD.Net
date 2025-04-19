// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Net.Domain;

public class RAiDDescriptionType
{
    [JsonPropertyName("id")] public required string Id { get; init; }

    [JsonPropertyName("schemaUri")] public required string SchemaUri { get; init; }
}
