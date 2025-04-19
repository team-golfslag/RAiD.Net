// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Net.Domain;

public class RAiDRelatedRaid
{
    [JsonPropertyName("id")] public string? Id { get; init; }

    [JsonPropertyName("type")] public RAiDRelatedRaidType? Type { get; init; }
}
