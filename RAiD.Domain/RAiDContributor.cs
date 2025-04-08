// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDContributor
{
    [JsonPropertyName("id")] public required string Id { get; set; }

    [JsonPropertyName("schemaUri")] public required string SchemaUri { get; set; }

    [JsonPropertyName("status")] public string? Status { get; set; }

    [JsonPropertyName("statusMessage")] public string? StatusMessage { get; set; }

    [JsonPropertyName("email")] public string? Email { get; set; }

    [JsonPropertyName("uuid")] public string? Uuid { get; set; }

    [JsonPropertyName("position")] public required List<RAiDContributorPosition> Position { get; set; }

    [JsonPropertyName("role")] public required List<RAiDContributorRole> Role { get; set; }

    [JsonPropertyName("leader")] public bool? Leader { get; set; }

    [JsonPropertyName("contact")] public bool? Contact { get; set; }
}
