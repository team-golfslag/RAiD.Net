// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDServicePointUpdateRequest
{
    [JsonPropertyName("id")] public required long Id { get; set; }

    [JsonPropertyName("name")] public required string Name { get; set; }

    [JsonPropertyName("adminEmail")] public string? AdminEmail { get; set; }

    [JsonPropertyName("techEmail")] public string? TechEmail { get; set; }

    [JsonPropertyName("identifierOwner")] public required string IdentifierOwner { get; set; }

    [JsonPropertyName("repositoryId")] public string? RepositoryId { get; set; }

    [JsonPropertyName("groupId")] public required string GroupId { get; set; }

    [JsonPropertyName("prefix")] public string? Prefix { get; set; }

    [JsonPropertyName("password")] public string? Password { get; set; }

    [JsonPropertyName("appWritesEnabled")] public bool? AppWritesEnabled { get; set; }

    [JsonPropertyName("enabled")] public bool? Enabled { get; set; }
}
