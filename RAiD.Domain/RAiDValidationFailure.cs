// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDValidationFailure
{
    [JsonPropertyName("fieldId")] public required string FieldId { get; init; }

    [JsonPropertyName("errorType")] public required string ErrorType { get; init; }

    [JsonPropertyName("message")] public required string Message { get; init; }
}
