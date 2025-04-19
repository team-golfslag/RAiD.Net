// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Net.Domain;

public class RAiDDescription
{
    [JsonPropertyName("text")] public required string Text { get; init; }

    [JsonPropertyName("type")] public required RAiDDescriptionType Type { get; init; }

    [JsonPropertyName("language")] public RAiDLanguage? Language { get; init; }
}
