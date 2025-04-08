// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDValidationFailureResponse
{
    [JsonPropertyName("failures")] public required List<RAiDValidationFailure> Failures { get; set; }

    [JsonPropertyName("type")] public required string Type { get; set; }

    [JsonPropertyName("title")] public required string Title { get; set; }

    [JsonPropertyName("status")] public required int Status { get; set; }

    [JsonPropertyName("detail")] public required string Detail { get; set; }

    [JsonPropertyName("instance")] public required string Instance { get; set; }
}
