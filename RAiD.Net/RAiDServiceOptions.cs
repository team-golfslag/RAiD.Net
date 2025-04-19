// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

namespace RAiD.Net;

/// <summary>
/// Configuration options for the RAiD HTTP client.
/// </summary>
public class RAiDServiceOptions
{
    /// <summary>
    /// The base URL for the RAiD service.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.demo.raid.org.au/";
}
