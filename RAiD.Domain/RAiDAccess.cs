// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDAccess
{
    [JsonPropertyName("type")] public required RAiDAccessType Type { get; set; }

    [JsonPropertyName("statement")] public RAiDAccessStatement? Statement { get; set; }

    [JsonPropertyName("embargoExpiry")] public string? EmbargoExpiry { get; set; }
}
