// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDContributor
{
    /// <summary>
    /// TODO: There is bad data in the RAiD database. The id is in one instance not present. This should be required!!
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("schemaUri")] public required string SchemaUri { get; init; }

    [JsonPropertyName("status")] public string? Status { get; init; }

    [JsonPropertyName("statusMessage")] public string? StatusMessage { get; init; }

    [JsonPropertyName("email")] public string? Email { get; init; }

    [JsonPropertyName("uuid")] public string? Uuid { get; init; }

    [JsonPropertyName("position")] public required List<RAiDContributorPosition> Position { get; init; }

    [JsonPropertyName("role")] public required List<RAiDContributorRole> Role { get; init; }

    [JsonPropertyName("leader")] public bool? Leader { get; init; }

    [JsonPropertyName("contact")] public bool? Contact { get; init; }
}
