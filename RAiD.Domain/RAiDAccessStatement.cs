// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDAccessStatement
{
    [JsonPropertyName("text")] public string? Text { get; set; }

    [JsonPropertyName("language")] public RAiDLanguage? Language { get; set; }
}
