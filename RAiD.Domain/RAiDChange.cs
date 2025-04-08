// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDChange
{
    [JsonPropertyName("handle")] public string? Handle { get; set; }

    [JsonPropertyName("version")] public int? Version { get; set; }

    [JsonPropertyName("diff")] public string? Diff { get; set; }

    [JsonPropertyName("timestamp")] public DateTime? Timestamp { get; set; }
}
