// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDPermissionsDto
{
    [JsonPropertyName("servicePointMatch")]
    public bool? ServicePointMatch { get; set; }

    [JsonPropertyName("read")] public bool? Read { get; set; }

    [JsonPropertyName("write")] public bool? Write { get; set; }
}
