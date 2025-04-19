// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Net.Domain;

public class RAiDAccess
{
    [JsonPropertyName("type")] public required RAiDAccessType Type { get; init; }

    [JsonPropertyName("statement")] public RAiDAccessStatement? Statement { get; init; }

    [JsonPropertyName("embargoExpiry")] public string? EmbargoExpiry { get; init; }
}
