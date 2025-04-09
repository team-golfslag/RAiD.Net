// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDClosedRaid
{
    [JsonPropertyName("identifier")] public RAiDId? Identifier { get; init; }

    [JsonPropertyName("access")] public RAiDAccess? Access { get; init; }
}
