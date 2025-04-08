// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDSpatialCoverage
{
    [JsonPropertyName("id")] public string? Id { get; init; }

    [JsonPropertyName("schemaUri")] public string? SchemaUri { get; init; }

    [JsonPropertyName("place")] public List<RAiDSpatialCoveragePlace>? Place { get; init; }
}
