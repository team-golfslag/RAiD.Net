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

    private string _authUrl;
    private string? _basicAuthUsername;
    private string? _basicAuthPassword;

    public RAiDService(
        HttpClient httpClient,
        IOptions<RAiDServiceOptions> options,
        ILogger<RAiDService> logger
    )
    {
        _httpClient = httpClient;
        _logger = logger;

        // configure base address from options
        _httpClient.BaseAddress = new(options.Value.BaseUrl);
        _authUrl = options.Value.AuthUrl;
    }

    /// <inheritdoc/>
    public void SetBearerToken(string bearerToken)
    {
        if (string.IsNullOrWhiteSpace(bearerToken))
            throw new ArgumentException(
                "Bearer token cannot be null or empty",
                nameof(bearerToken)
            );

        _bearerToken = bearerToken;
        _httpClient.DefaultRequestHeaders.Authorization = new(
            "Bearer",
            bearerToken
        );
    }

    public async Task SetUsernameAndPassword(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException(
                "Username cannot be null or empty",
                nameof(username)
            );
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException(
                "Password cannot be null or empty",
                nameof(password)
            );

        _basicAuthPassword = password;
        _basicAuthUsername = username;

        HttpRequestMessage request = new(HttpMethod.Post, _authUrl)
        {
            Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "client_id", "raid-api" },
                    { "username", username },
                    { "password", password },
                    { "grant_type", "password" }
                }
            ),
        };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(
                e,
                "Authentication request failed for user {Username}",
                username
            );
            _basicAuthUsername = null;
            _basicAuthPassword = null;
            throw new RAiDException("Authentication request failed", e);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            var ex = new HttpRequestException(
                $"Authentication failed with status code {response.StatusCode}. Response: {errorBody}",
                null,
                response.StatusCode
            );

            _logger.LogError(
                ex,
                "Authentication failed for user {Username} with body: {Body}",
                username,
                errorBody
            );
            _basicAuthUsername = null;
            _basicAuthPassword = null;

            throw new RAiDException(
                $"Authentication failed. Response: {errorBody}",
                ex
            );
        }

        try
        {
            RAiDAuthResponse authResponse =
                await response.Content.ReadFromJsonAsync<RAiDAuthResponse>()
                ?? throw new RAiDException("Invalid authentication response");

            SetBearerToken(authResponse.AccessToken);
        }
        catch (JsonException e)
        {
            _logger.LogError(
                e,
                "Deserializing authentication response failed"
            );
            _basicAuthUsername = null;
            _basicAuthPassword = null;
            throw new RAiDException(
                "Invalid JSON in authentication response",
                e
            );
        }
    }

    private async Task<T?> SendGetAsync<T>(string url, bool retry = false)
    {
        HttpResponseMessage resp;
        try
        {
            resp = await _httpClient.GetAsync(url);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "GET {Url} failed", url);
            throw new RAiDException($"GET {url} failed", e);
        }

        if (!resp.IsSuccessStatusCode)
        {
            if (
                resp.StatusCode == HttpStatusCode.Unauthorized
                && _basicAuthUsername != null
                && !retry
            )
            {
                await SetUsernameAndPassword(
                    _basicAuthUsername,
                    _basicAuthPassword!
                );
                // Retry the request after re-authentication
                return await SendGetAsync<T>(url, true);
            }

            var errorBody = await resp.Content.ReadAsStringAsync();
            var ex = new HttpRequestException(
                $"Request failed with status code {resp.StatusCode}. Response: {errorBody}",
                null,
                resp.StatusCode
            );

            _logger.LogError(
                ex,
                "GET {Url} failed with body: {Body}",
                url,
                errorBody
            );
            throw new RAiDException(
                $"GET {url} failed. Response: {errorBody}",
                ex
            );
        }

        try
        {
            return await resp.Content.ReadFromJsonAsync<T>();
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Deserializing GET {Url} failed", url);
            throw new RAiDException(
                $"Invalid JSON in response from {url}",
                e
            );
        }
    }

    private async Task<TResp?> SendJsonAsync<TReq, TResp>(
        string method,
        string url,
        TReq payload,
        bool retry = false
    )
    {
        HttpResponseMessage resp;
        try
        {
            resp = method switch
            {
                "POST" => await _httpClient.PostAsJsonAsync(url, payload),
                "PUT"  => await _httpClient.PutAsJsonAsync(url, payload),
                "PATCH"
                    => await _httpClient.SendAsync(
                        new(HttpMethod.Patch, url)
                        {
                            Content = JsonContent.Create(payload)
                        }
                    ),
                _
                    => throw new InvalidOperationException(
                        $"Unsupported HTTP method {method}"
                    )
            };
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "{Method} {Url} failed", method, url);
            throw new RAiDException($"{method} {url} failed", e);
        }

        if (!resp.IsSuccessStatusCode)
        {
            if (
                resp.StatusCode == HttpStatusCode.Unauthorized
                && _basicAuthUsername != null
                && !retry
            )
            {
                await SetUsernameAndPassword(
                    _basicAuthUsername,
                    _basicAuthPassword!
                );
                return await SendJsonAsync<TReq, TResp>(
                    method,
                    url,
                    payload,
                    true
                );
            }

            var errorBody = await resp.Content.ReadAsStringAsync();
            var ex = new HttpRequestException(
                $"Request failed with status code {resp.StatusCode}. Response: {errorBody}",
                null,
                resp.StatusCode
            );

            _logger.LogError(
                ex,
                "{Method} {Url} failed with body: {Body}",
                method,
                url,
                errorBody
            );
            throw new RAiDException(
                $"{method} {url} failed. Response: {errorBody}",
                ex
            );
        }

        try
        {
            return await resp.Content.ReadFromJsonAsync<TResp>();
        }
        catch (JsonException e)
        {
            _logger.LogError(
                e,
                "{Method} {Url} returned invalid JSON",
                method,
                url
            );
            throw new RAiDException(
                $"Invalid JSON in response from {url}",
                e
            );
        }
    }

    #region ServicePoint endpoints

    /// <inheritdoc/>
    public Task<RAiDServicePoint?> FindServicePointByIdAsync(long id) =>
        SendGetAsync<RAiDServicePoint>($"/service-point/{id}");

    /// <inheritdoc/>
    public Task<RAiDServicePoint?> UpdateServicePointAsync(
        long id,
        RAiDServicePointUpdateRequest updateRequest
    ) =>
        SendJsonAsync<RAiDServicePointUpdateRequest, RAiDServicePoint>(
            "PUT",
            $"/service-point/{id}",
            updateRequest
        );

    /// <inheritdoc/>
    public Task<List<RAiDServicePoint>?> FindAllServicePointsAsync() =>
        SendGetAsync<List<RAiDServicePoint>>("/service-point/");

    /// <inheritdoc/>
    public Task<RAiDServicePoint?> CreateServicePointAsync(
        RAiDServicePointCreateRequest createRequest
    ) =>
        SendJsonAsync<RAiDServicePointCreateRequest, RAiDServicePoint>(
            "POST",
            "/service-point/",
            createRequest
        );

    #endregion

    #region RAiD endpoints

    /// <inheritdoc/>
    public Task<RAiDDto?> FindRaidByNameAsync(string prefix, string suffix) =>
        SendGetAsync<RAiDDto>($"/raid/{prefix}/{suffix}");

    /// <inheritdoc/>
    public Task<RAiDDto?> UpdateRaidAsync(
        string prefix,
        string suffix,
        RAiDUpdateRequest updateRequest
    ) =>
        SendJsonAsync<RAiDUpdateRequest, RAiDDto>(
            "PUT",
            $"/raid/{prefix}/{suffix}",
            updateRequest
        );

    /// <inheritdoc/>
    public Task<RAiDDto?> PatchRaidAsync(
        string prefix,
        string suffix,
        RAiDPatchRequest patchRequest
    ) =>
        SendJsonAsync<RAiDPatchRequest, RAiDDto>(
            "PATCH",
            $"/raid/{prefix}/{suffix}",
            patchRequest
        );

    /// <inheritdoc/>
    public Task<List<RAiDDto>?> FindAllRaidsAsync(
        IEnumerable<string>? includeFields = null,
        string? contributorId = null,
        string? organisationId = null
    )
    {
        var qp = new List<string>();
        if (includeFields != null)
            qp.AddRange(
                includeFields.Select(f =>
                    $"includeFields={Uri.EscapeDataString(f)}"
                )
            );
        if (!string.IsNullOrEmpty(contributorId))
            qp.Add(
                $"contributor.id={Uri.EscapeDataString(contributorId)}"
            );
        if (!string.IsNullOrEmpty(organisationId))
            qp.Add(
                $"organisation.id={Uri.EscapeDataString(organisationId)}"
            );

        string suffix = qp.Count > 0 ? "?" + string.Join("&", qp) : string.Empty;
        return SendGetAsync<List<RAiDDto>>($"/raid/{suffix}");
    }

    /// <inheritdoc/>
    public Task<RAiDDto?> MintRaidAsync(RAiDCreateRequest createRequest) =>
        SendJsonAsync<RAiDCreateRequest, RAiDDto>(
            "POST",
            "/raid/",
            createRequest
        );

    /// <inheritdoc/>
    public Task<List<RAiDDto>?> FindAllPublicRaidsAsync() => SendGetAsync<List<RAiDDto>>("/raid/all-public");

    /// <inheritdoc/>
    public Task<object?> FindRaidByNameAndVersionAsync(
        string prefix,
        string suffix,
        int version
    ) =>
        SendGetAsync<object>($"/raid/{prefix}/{suffix}/{version}");

    /// <inheritdoc/>
    public Task<RAiDPermissionsDto?> GetRaidPermissionsAsync(
        string prefix,
        string suffix
    ) =>
        SendGetAsync<RAiDPermissionsDto>($"/raid/{prefix}/{suffix}/permissions");

    /// <inheritdoc/>
    public Task<List<RAiDChange>?> GetRaidHistoryAsync(
        string prefix,
        string suffix
    ) =>
        SendGetAsync<List<RAiDChange>>($"/raid/{prefix}/{suffix}/history");

    #endregion

    #region Upgrade endpoints

    /// <inheritdoc/>
    public Task<RAiDDto?> UpgradeRaidAsync(RAiDUpdateRequest updateRequest) =>
        SendJsonAsync<RAiDUpdateRequest, RAiDDto>(
            "POST",
            "/upgrade",
            updateRequest
        );

    /// <inheritdoc/>
    public Task<List<RAiDDto>?> FindAllUpgradableAsync() => SendGetAsync<List<RAiDDto>>("/upgradable/all");

    #endregion
}
