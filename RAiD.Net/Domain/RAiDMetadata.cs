// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Net.Domain;

public class RAiDMetadata
{
    [JsonPropertyName("created")] public double? Created { get; init; }

    [JsonPropertyName("updated")] public double? Updated { get; init; }
}
