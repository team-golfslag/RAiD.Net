// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDChange
{
    [JsonPropertyName("handle")] public string? Handle { get; init; }

    [JsonPropertyName("version")] public int? Version { get; init; }

    [JsonPropertyName("diff")] public string? Diff { get; init; }

    [JsonPropertyName("timestamp")] public DateTime? Timestamp { get; init; }
}
