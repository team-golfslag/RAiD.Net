// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Net.Domain;

public class RAiDServicePointCreateRequest
{
    [JsonPropertyName("name")] public required string Name { get; init; }

    [JsonPropertyName("adminEmail")] public string? AdminEmail { get; init; }

    [JsonPropertyName("techEmail")] public string? TechEmail { get; init; }

    [JsonPropertyName("identifierOwner")] public required string IdentifierOwner { get; init; }

    [JsonPropertyName("repositoryId")] public string? RepositoryId { get; init; }

    [JsonPropertyName("groupId")] public required string GroupId { get; init; }

    [JsonPropertyName("prefix")] public string? Prefix { get; init; }

    [JsonPropertyName("password")] public string? Password { get; init; }

    [JsonPropertyName("appWritesEnabled")] public bool? AppWritesEnabled { get; init; }

    [JsonPropertyName("enabled")] public bool? Enabled { get; init; }
}
