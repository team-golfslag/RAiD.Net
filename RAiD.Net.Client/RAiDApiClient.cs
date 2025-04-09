using System.Net.Http.Json;
using RAiD.Domain;

namespace RAiD.Client;

public class RAiDApiClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Pass a configured HttpClient (with BaseAddress set) into the constructor.
    /// Example usage:
    /// <code>
    ///   var httpClient = new HttpClient
    ///   {
    ///       BaseAddress = new Uri("https://YOUR-API-ENDPOINT/")
    ///   };
    ///   var client = new RaidApiClient(httpClient);
    /// </code>
    /// </summary>
    public RAiDApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Sets the bearer token for all subsequent requests.
    /// Call this before calling other methods if authentication is required.
    /// </summary>
    public void SetBearerToken(string bearerToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new("Bearer", bearerToken);
    }

    #region ServicePoint endpoints

    /// <summary>
    /// GET /service-point/{id}
    /// Retrieve a ServicePoint by ID.
    /// </summary>
    public async Task<RAiDServicePoint?> FindServicePointByIdAsync(long id)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"/service-point/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RAiDServicePoint>();
    }

    /// <summary>
    /// PUT /service-point/{id}
    /// Update a ServicePoint by ID.
    /// </summary>
    public async Task<RAiDServicePoint?> UpdateServicePointAsync(long id, RAiDServicePointUpdateRequest updateRequest)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"/service-point/{id}", updateRequest);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RAiDServicePoint>();
    }

    /// <summary>
    /// GET /service-point/
    /// Returns all ServicePoints.
    /// </summary>
    public async Task<List<RAiDServicePoint>?> FindAllServicePointsAsync()
    {
        HttpResponseMessage response = await _httpClient.GetAsync("/service-point/");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<RAiDServicePoint>>();
    }

    /// <summary>
    /// POST /service-point/
    /// Creates a new ServicePoint.
    /// </summary>
    public async Task<RAiDServicePoint?> CreateServicePointAsync(RAiDServicePointCreateRequest createRequest)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/service-point/", createRequest);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RAiDServicePoint>();
    }

    #endregion

    #region RAiD endpoints

    /// <summary>
    /// GET /raid/{prefix}/{suffix}
    /// Read a specific RAiD by prefix/suffix.
    /// </summary>
    public async Task<RAiDDto?> FindRaidByNameAsync(string prefix, string suffix)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"/raid/{prefix}/{suffix}");
        if (response.IsSuccessStatusCode)
            // 200 => RAiDDto
            return await response.Content.ReadFromJsonAsync<RAiDDto>();

        response.EnsureSuccessStatusCode();
        return null;
    }

    /// <summary>
    /// PUT /raid/{prefix}/{suffix}
    /// Update a RAiD.
    /// </summary>
    public async Task<RAiDDto?> UpdateRaidAsync(string prefix, string suffix, RAiDUpdateRequest updateRequest)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"/raid/{prefix}/{suffix}", updateRequest);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RAiDDto>();
    }

    /// <summary>
    /// PATCH /raid/{prefix}/{suffix}
    /// Patch a RAiD.
    /// </summary>
    public async Task<RAiDDto?> PatchRaidAsync(string prefix, string suffix, RAiDPatchRequest patchRequest)
    {
        // HttpClient doesn't have PatchAsync by default, so we manually craft the request.
        HttpRequestMessage request = new(new("PATCH"), $"/raid/{prefix}/{suffix}")
        {
            Content = JsonContent.Create(patchRequest),
        };

        HttpResponseMessage response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RAiDDto>();
    }

    /// <summary>
    /// GET /raid/
    /// List RAiDs (with optional query params).
    /// </summary>
    public async Task<List<RAiDDto>?> FindAllRaidsAsync(
        IEnumerable<string>? includeFields = null,
        string? contributorId = null,
        string? organisationId = null)
    {
        // Build query string
        // e.g. /raid/?includeFields=title&includeFields=date&contributor.id=abc
        var queryParams = new List<string>();

        if (includeFields != null)
            queryParams.AddRange(includeFields.Select(field => $"includeFields={Uri.EscapeDataString(field)}"));
        if (!string.IsNullOrEmpty(contributorId))
            queryParams.Add($"contributor.id={Uri.EscapeDataString(contributorId)}");
        if (!string.IsNullOrEmpty(organisationId))
            queryParams.Add($"organisation.id={Uri.EscapeDataString(organisationId)}");

        string queryString = queryParams.Count > 0
            ? "?" + string.Join("&", queryParams)
            : string.Empty;

        HttpResponseMessage response = await _httpClient.GetAsync("/raid/" + queryString);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<RAiDDto>>();
    }

    /// <summary>
    /// POST /raid/
    /// Mint (create) a new RAiD.
    /// </summary>
    public async Task<RAiDDto?> MintRaidAsync(RAiDCreateRequest createRequest)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/raid/", createRequest);
        // 201 => RAiDDto, 400 => ValidationFailureResponse
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RAiDDto>();
    }

    /// <summary>
    /// GET /raid/all-public
    /// List all public RAiDs.
    /// </summary>
    public async Task<List<RAiDDto>?> FindAllPublicRaidsAsync()
    {
        HttpResponseMessage response = await _httpClient.GetAsync("/raid/all-public");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<RAiDDto>>();
    }

    /// <summary>
    /// GET /raid/{prefix}/{suffix}/{version}
    /// Read a RAiD at a specific version number.
    /// The spec shows a plain object; adjust if you have a known shape.
    /// </summary>
    public async Task<object?> FindRaidByNameAndVersionAsync(string prefix, string suffix, int version)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"/raid/{prefix}/{suffix}/{version}");
        if (response.IsSuccessStatusCode)
            // Could parse as an object or a custom class, depending on your data.
            return await response.Content.ReadFromJsonAsync<object>();

        response.EnsureSuccessStatusCode();
        return null;
    }

    /// <summary>
    /// GET /raid/{prefix}/{suffix}/permissions
    /// Retrieve RAiD permissions.
    /// </summary>
    public async Task<RAiDPermissionsDto?> GetRaidPermissionsAsync(string prefix, string suffix)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"/raid/{prefix}/{suffix}/permissions");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RAiDPermissionsDto>();
    }

    /// <summary>
    /// GET /raid/{prefix}/{suffix}/history
    /// Returns JSON patch change history for the RAiD.
    /// </summary>
    public async Task<List<RAiDChange>?> GetRaidHistoryAsync(string prefix, string suffix)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"/raid/{prefix}/{suffix}/history");
        if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<List<RAiDChange>>();

        response.EnsureSuccessStatusCode();
        return null;
    }

    #endregion

    #region Upgrade endpoints

    /// <summary>
    /// POST /upgrade
    /// Upgrades a RAiD, returning the updated RAiDDto.
    /// </summary>
    public async Task<RAiDDto?> UpgradeRaidAsync(RAiDUpdateRequest updateRequest)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/upgrade", updateRequest);
        // 200 => RAiDDto
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RAiDDto>();
    }

    /// <summary>
    /// GET /upgradable/all
    /// Returns a list of RAiDs that are upgradable.
    /// </summary>
    public async Task<List<RAiDDto>?> FindAllUpgradableAsync()
    {
        HttpResponseMessage response = await _httpClient.GetAsync("/upgradable/all");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<RAiDDto>>();
    }

    #endregion
}
