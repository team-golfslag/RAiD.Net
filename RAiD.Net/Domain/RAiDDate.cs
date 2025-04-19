// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;
using RAiD.Net.Helpers;

namespace RAiD.Net.Domain;

public class RAiDDate
{
    [JsonConverter(typeof(RAiDDateConverter))]
    [JsonPropertyName("startDate")]
    public required DateTime StartDate { get; init; }

    [JsonConverter(typeof(RAiDDateConverter))]
    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; init; }
}
