// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Net.Domain;

public class RAiDUpdateRequest
{
    [JsonPropertyName("metadata")] public RAiDMetadata? Metadata { get; init; }

    [JsonPropertyName("identifier")] public required RAiDId Identifier { get; init; }

    [JsonPropertyName("title")] public List<RAiDTitle>? Title { get; init; }

    [JsonPropertyName("date")] public RAiDDate? Date { get; init; }

    [JsonPropertyName("description")] public List<RAiDDescription>? Description { get; init; }

    [JsonPropertyName("access")] public required RAiDAccess Access { get; init; }

    [JsonPropertyName("alternateUrl")] public List<RAiDAlternateUrl>? AlternateUrl { get; init; }

    [JsonPropertyName("contributor")] public List<RAiDContributor>? Contributor { get; init; }

    [JsonPropertyName("organisation")] public List<RAiDOrganisation>? Organisation { get; init; }

    [JsonPropertyName("subject")] public List<RAiDSubject>? Subject { get; init; }

    [JsonPropertyName("relatedRaid")] public List<RAiDRelatedRaid>? RelatedRaid { get; init; }

    [JsonPropertyName("relatedObject")] public List<RAiDRelatedObject>? RelatedObject { get; init; }

    [JsonPropertyName("alternateIdentifier")]
    public List<RAiDAlternateIdentifier>? AlternateIdentifier { get; init; }

    [JsonPropertyName("spatialCoverage")] public List<RAiDSpatialCoverage>? SpatialCoverage { get; init; }
}
