// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RAiD.Net.Domain;
using RAiD.Net.Exceptions;

namespace RAiD.Net;

public class RAiDService : IRAiDService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RAiDService> _logger;

    public RAiDService(
        HttpClient httpClient,
        IOptions<RAiDServiceOptions> options,
        ILogger<RAiDService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // configure base address from options
        _httpClient.BaseAddress = new(options.Value.BaseUrl);
    }

    /// <inheritdoc/>
    public void SetBearerToken(string bearerToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new("Bearer", bearerToken);
    }

    private async Task<T?> SendGetAsync<T>(string url)
    {
        try
        {
            HttpResponseMessage resp = await _httpClient.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<T>();
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "GET {Url} failed", url);
            throw new RAiDException($"GET {url} failed", e);
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Deserializing GET {Url} failed", url);
            throw new RAiDException($"Invalid JSON in response from {url}", e);
        }
    }

    private async Task<TResp?> SendJsonAsync<TReq, TResp>(string method, string url, TReq payload)
    {
        try
        {
            HttpResponseMessage resp = method switch
            {
                "POST" => await _httpClient.PostAsJsonAsync(url, payload),
                "PUT"  => await _httpClient.PutAsJsonAsync(url, payload),
                "PATCH" =>
                    await _httpClient.SendAsync(new(HttpMethod.Patch, url)
                    {
                        Content = JsonContent.Create(payload)
                    }),
                _ => throw new InvalidOperationException($"Unsupported HTTP method {method}")
            };

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<TResp>();
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "{Method} {Url} failed", method, url);
            throw new RAiDException($"{method} {url} failed", e);
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "{Method} {Url} returned invalid JSON", method, url);
            throw new RAiDException($"Invalid JSON in response from {url}", e);
        }
    }

    #region ServicePoint endpoints

    /// <inheritdoc/>
    public Task<RAiDServicePoint?> FindServicePointByIdAsync(long id)
        => SendGetAsync<RAiDServicePoint>($"/service-point/{id}");

    /// <inheritdoc/>
    public Task<RAiDServicePoint?> UpdateServicePointAsync(long id, RAiDServicePointUpdateRequest updateRequest)
        => SendJsonAsync<RAiDServicePointUpdateRequest, RAiDServicePoint>(
            "PUT", $"/service-point/{id}", updateRequest);

    /// <inheritdoc/>
    public Task<List<RAiDServicePoint>?> FindAllServicePointsAsync()
        => SendGetAsync<List<RAiDServicePoint>>("/service-point/");

    /// <inheritdoc/>
    public Task<RAiDServicePoint?> CreateServicePointAsync(RAiDServicePointCreateRequest createRequest)
        => SendJsonAsync<RAiDServicePointCreateRequest, RAiDServicePoint>(
            "POST", "/service-point/", createRequest);

    #endregion

    #region RAiD endpoints

    /// <inheritdoc/>
    public Task<RAiDDto?> FindRaidByNameAsync(string prefix, string suffix)
        => SendGetAsync<RAiDDto>($"/raid/{prefix}/{suffix}");

    /// <inheritdoc/>
    public Task<RAiDDto?> UpdateRaidAsync(string prefix, string suffix, RAiDUpdateRequest updateRequest)
        => SendJsonAsync<RAiDUpdateRequest, RAiDDto>(
            "PUT", $"/raid/{prefix}/{suffix}", updateRequest);

    /// <inheritdoc/>
    public Task<RAiDDto?> PatchRaidAsync(string prefix, string suffix, RAiDPatchRequest patchRequest)
        => SendJsonAsync<RAiDPatchRequest, RAiDDto>(
            "PATCH", $"/raid/{prefix}/{suffix}", patchRequest);

    /// <inheritdoc/>
    public Task<List<RAiDDto>?> FindAllRaidsAsync(
        IEnumerable<string>? includeFields = null,
        string? contributorId = null,
        string? organisationId = null)
    {
        var qp = new List<string>();
        if (includeFields != null)
            qp.AddRange(includeFields.Select(f => $"includeFields={Uri.EscapeDataString(f)}"));
        if (!string.IsNullOrEmpty(contributorId))
            qp.Add($"contributor.id={Uri.EscapeDataString(contributorId)}");
        if (!string.IsNullOrEmpty(organisationId))
            qp.Add($"organisation.id={Uri.EscapeDataString(organisationId)}");

        string suffix = qp.Count > 0 ? "?" + string.Join("&", qp) : string.Empty;
        return SendGetAsync<List<RAiDDto>>($"/raid/{suffix}");
    }

    /// <inheritdoc/>
    public Task<RAiDDto?> MintRaidAsync(RAiDCreateRequest createRequest)
        => SendJsonAsync<RAiDCreateRequest, RAiDDto>("POST", "/raid/", createRequest);

    /// <inheritdoc/>
    public Task<List<RAiDDto>?> FindAllPublicRaidsAsync()
        => SendGetAsync<List<RAiDDto>>("/raid/all-public");

    /// <inheritdoc/>
    public Task<object?> FindRaidByNameAndVersionAsync(string prefix, string suffix, int version)
        => SendGetAsync<object>($"/raid/{prefix}/{suffix}/{version}");

    /// <inheritdoc/>
    public Task<RAiDPermissionsDto?> GetRaidPermissionsAsync(string prefix, string suffix)
        => SendGetAsync<RAiDPermissionsDto>($"/raid/{prefix}/{suffix}/permissions");

    /// <inheritdoc/>
    public Task<List<RAiDChange>?> GetRaidHistoryAsync(string prefix, string suffix)
        => SendGetAsync<List<RAiDChange>>($"/raid/{prefix}/{suffix}/history");

    #endregion

    #region Upgrade endpoints

    /// <inheritdoc/>
    public Task<RAiDDto?> UpgradeRaidAsync(RAiDUpdateRequest updateRequest)
        => SendJsonAsync<RAiDUpdateRequest, RAiDDto>("POST", "/upgrade", updateRequest);

    /// <inheritdoc/>
    public Task<List<RAiDDto>?> FindAllUpgradableAsync()
        => SendGetAsync<List<RAiDDto>>("/upgradable/all");

    #endregion
}