// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Net.Domain;

public class RAiDId
{
    [JsonPropertyName("id")] public required string IdValue { get; init; }

    [JsonPropertyName("schemaUri")] public required string SchemaUri { get; init; }

    [JsonPropertyName("registrationAgency")]
    public required RAiDRegistrationAgency RegistrationAgency { get; init; }

    [JsonPropertyName("owner")] public required RAiDOwner Owner { get; init; }

    [JsonPropertyName("raidAgencyUrl")] public string? RaidAgencyUrl { get; init; }

    [JsonPropertyName("license")] public required string License { get; init; }

    [JsonPropertyName("version")] public required int Version { get; init; }
}
