// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDServicePoint
{
    [JsonPropertyName("id")] public required long Id { get; init; }

    [JsonPropertyName("name")] public required string Name { get; init; }

    [JsonPropertyName("identifierOwner")] public required string IdentifierOwner { get; init; }

    [JsonPropertyName("repositoryId")] public string? RepositoryId { get; init; }

    [JsonPropertyName("prefix")] public string? Prefix { get; init; }

    [JsonPropertyName("groupId")] public string? GroupId { get; init; }

    [JsonPropertyName("searchContent")] public string? SearchContent { get; init; }

    [JsonPropertyName("techEmail")] public required string TechEmail { get; init; }

    [JsonPropertyName("adminEmail")] public required string AdminEmail { get; init; }

    [JsonPropertyName("enabled")] public required bool Enabled { get; init; }

    [JsonPropertyName("appWritesEnabled")] public bool? AppWritesEnabled { get; init; }
}
