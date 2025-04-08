// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Text.Json.Serialization;

namespace RAiD.Domain;

public class RAiDCreateRequest
{
    [JsonPropertyName("metadata")] public RAiDMetadata? Metadata { get; set; }

    [JsonPropertyName("identifier")] public RAiDId? Identifier { get; set; }

    [JsonPropertyName("title")] public List<RAiDTitle>? Title { get; set; }

    [JsonPropertyName("date")] public RAiDDate? Date { get; set; }

    [JsonPropertyName("description")] public List<RAiDDescription>? Description { get; set; }

    [JsonPropertyName("access")] public required RAiDAccess Access { get; set; }

    [JsonPropertyName("alternateUrl")] public List<RAiDAlternateUrl>? AlternateUrl { get; set; }

    [JsonPropertyName("contributor")] public List<RAiDContributor>? Contributor { get; set; }

    [JsonPropertyName("organisation")] public List<RAiDOrganisation>? Organisation { get; set; }

    [JsonPropertyName("subject")] public List<RAiDSubject>? Subject { get; set; }

    [JsonPropertyName("relatedRaid")] public List<RAiDRelatedRaid>? RelatedRaid { get; set; }

    [JsonPropertyName("relatedObject")] public List<RAiDRelatedObject>? RelatedObject { get; set; }

    [JsonPropertyName("alternateIdentifier")]
    public List<RAiDAlternateIdentifier>? AlternateIdentifier { get; set; }

    [JsonPropertyName("spatialCoverage")] public List<RAiDSpatialCoverage>? SpatialCoverage { get; set; }
}
