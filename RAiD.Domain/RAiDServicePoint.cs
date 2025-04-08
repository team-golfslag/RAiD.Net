// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDServicePoint
{
    [JsonPropertyName("id")] public required long Id { get; set; }

    [JsonPropertyName("name")] public required string Name { get; set; }

    [JsonPropertyName("identifierOwner")] public required string IdentifierOwner { get; set; }

    [JsonPropertyName("repositoryId")] public string? RepositoryId { get; set; }

    [JsonPropertyName("prefix")] public string? Prefix { get; set; }

    [JsonPropertyName("groupId")] public string? GroupId { get; set; }

    [JsonPropertyName("searchContent")] public string? SearchContent { get; set; }

    [JsonPropertyName("techEmail")] public required string TechEmail { get; set; }

    [JsonPropertyName("adminEmail")] public required string AdminEmail { get; set; }

    [JsonPropertyName("enabled")] public required bool Enabled { get; set; }

    [JsonPropertyName("appWritesEnabled")] public bool? AppWritesEnabled { get; set; }
}
