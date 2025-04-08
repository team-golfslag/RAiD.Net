// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;
using RAiD.Client.Helpers;

namespace RAiD.Domain;

public class RAiDTitle
{
    [JsonPropertyName("text")] public required string Text { get; init; }

    [JsonPropertyName("type")] public required RAiDTitleType Type { get; init; }

    [JsonConverter(typeof(RAiDDateConverter))]
    [JsonPropertyName("startDate")]
    public required DateTime StartDate { get; init; }

    [JsonConverter(typeof(RAiDDateConverter))]
    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; init; }

    [JsonPropertyName("language")] public RAiDLanguage? Language { get; init; }
}
