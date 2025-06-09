// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using System.Net;
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
    private string? _bearerToken;
    
    private string? _basicAuthUsername;
    private string? _basicAuthPassword;
    private string? _authUrl;

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
        if (string.IsNullOrWhiteSpace(bearerToken))
            throw new ArgumentException("Bearer token cannot be null or empty", nameof(bearerToken));

        _bearerToken = bearerToken;
        _httpClient.DefaultRequestHeaders.Authorization =
            new("Bearer", bearerToken);
    }

    public async Task SetUsernameAndPassword(string authUrl, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(authUrl))
            throw new ArgumentException("Authentication URL cannot be null or empty", nameof(authUrl));
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        
        _authUrl = authUrl;
        _basicAuthPassword = password;
        _basicAuthUsername = username;

        HttpRequestMessage request = new(HttpMethod.Post, _authUrl)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", "raid-api" },
                { "username", username },
                { "password", password },
                { "grant_type", "password" }
            }),
        };
        
        try
        {
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            RAiDAuthResponse authResponse = await response.Content.ReadFromJsonAsync<RAiDAuthResponse>()
                ?? throw new RAiDException("Invalid authentication response");

            SetBearerToken(authResponse.AccessToken);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Authentication failed for user {Username}", username);
            _authUrl = null;
            _basicAuthUsername = null;
            _basicAuthPassword = null;
            
            throw new RAiDException("Authentication failed", e);
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Deserializing authentication response failed");
            throw new RAiDException("Invalid JSON in authentication response", e);
        }
    }

    private async Task<T?> SendGetAsync<T>(string url, bool retry = false)
    {
        try
        {
            HttpResponseMessage resp = await _httpClient.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<T>();
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == HttpStatusCode.Unauthorized && _authUrl != null && !retry)
            {
                await SetUsernameAndPassword(_authUrl, _basicAuthUsername!, _basicAuthPassword!);
                // Retry the request after re-authentication
                return await SendGetAsync<T>(url, true);
            }

            _logger.LogError(e, "GET {Url} failed", url);
            throw new RAiDException($"GET {url} failed", e);
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Deserializing GET {Url} failed", url);
            throw new RAiDException($"Invalid JSON in response from {url}", e);
        }
    }

    private async Task<TResp?> SendJsonAsync<TReq, TResp>(string method, string url, TReq payload, bool retry = false)
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
            if (e.StatusCode == HttpStatusCode.Unauthorized && _authUrl != null && !retry)
            {
                await SetUsernameAndPassword(_authUrl, _basicAuthUsername!, _basicAuthPassword!);
                return await SendJsonAsync<TReq, TResp>(method, url, payload, true);
            }
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