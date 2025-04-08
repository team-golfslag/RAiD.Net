// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDId
{
    [JsonPropertyName("id")] public required string IdValue { get; set; }

    [JsonPropertyName("schemaUri")] public required string SchemaUri { get; set; }

    [JsonPropertyName("registrationAgency")]
    public required RAiDRegistrationAgency RegistrationAgency { get; set; }

    [JsonPropertyName("owner")] public required RAiDOwner Owner { get; set; }

    [JsonPropertyName("raidAgencyUrl")] public string? RaidAgencyUrl { get; set; }

    [JsonPropertyName("license")] public required string License { get; set; }

    [JsonPropertyName("version")] public required int Version { get; set; }
}
