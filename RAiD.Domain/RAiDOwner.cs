// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDOwner
{
    [JsonPropertyName("id")] public required string Id { get; set; }

    [JsonPropertyName("schemaUri")] public required string SchemaUri { get; set; }

    [JsonPropertyName("servicePoint")] public long? ServicePoint { get; set; }
}
