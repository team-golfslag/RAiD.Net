// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDValidationFailureResponse
{
    [JsonPropertyName("failures")] public required List<RAiDValidationFailure> Failures { get; init; }

    [JsonPropertyName("type")] public required string Type { get; init; }

    [JsonPropertyName("title")] public required string Title { get; init; }

    [JsonPropertyName("status")] public required int Status { get; init; }

    [JsonPropertyName("detail")] public required string Detail { get; init; }

    [JsonPropertyName("instance")] public required string Instance { get; init; }
}
