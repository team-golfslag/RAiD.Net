// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

using RAiD.Net.Domain;

namespace RAiD.Net;

/// <summary>
/// Service to interact with the RAiD API.
/// </summary>
public interface IRAiDService
{
    /// <summary>
    /// Sets the bearer token for all subsequent requests.
    /// Call this before calling other methods if authentication is required.
    /// </summary>
    void SetBearerToken(string bearerToken);

    #region ServicePoint endpoints

    /// <summary>
    /// GET /service-point/{id}
    /// Retrieve a ServicePoint by ID.
    /// </summary>
    Task<RAiDServicePoint?> FindServicePointByIdAsync(long id);

    /// <summary>
    /// PUT /service-point/{id}
    /// Update a ServicePoint by ID.
    /// </summary>
    Task<RAiDServicePoint?> UpdateServicePointAsync(long id, RAiDServicePointUpdateRequest updateRequest);

    /// <summary>
    /// GET /service-point/
    /// Returns all ServicePoints.
    /// </summary>
    Task<List<RAiDServicePoint>?> FindAllServicePointsAsync();

    /// <summary>
    /// POST /service-point/
    /// Creates a new ServicePoint.
    /// </summary>
    Task<RAiDServicePoint?> CreateServicePointAsync(RAiDServicePointCreateRequest createRequest);

    #endregion

    #region RAiD endpoints

    /// <summary>
    /// GET /raid/{prefix}/{suffix}
    /// Read a specific RAiD by prefix/suffix.
    /// </summary>
    Task<RAiDDto?> FindRaidByNameAsync(string prefix, string suffix);

    /// <summary>
    /// PUT /raid/{prefix}/{suffix}
    /// Update a RAiD.
    /// </summary>
    Task<RAiDDto?> UpdateRaidAsync(string prefix, string suffix, RAiDUpdateRequest updateRequest);

    /// <summary>
    /// PATCH /raid/{prefix}/{suffix}
    /// Patch a RAiD.
    /// </summary>
    Task<RAiDDto?> PatchRaidAsync(string prefix, string suffix, RAiDPatchRequest patchRequest);

    /// <summary>
    /// GET /raid/
    /// List RAiDs (with optional query params).
    /// </summary>
    Task<List<RAiDDto>?> FindAllRaidsAsync(
        IEnumerable<string>? includeFields = null,
        string? contributorId = null,
        string? organisationId = null);

    /// <summary>
    /// POST /raid/
    /// Mint (create) a new RAiD.
    /// </summary>
    Task<RAiDDto?> MintRaidAsync(RAiDCreateRequest createRequest);

    /// <summary>
    /// GET /raid/all-public
    /// List all public RAiDs.
    /// </summary>
    Task<List<RAiDDto>?> FindAllPublicRaidsAsync();

    /// <summary>
    /// GET /raid/{prefix}/{suffix}/{version}
    /// Read a RAiD at a specific version number.
    /// </summary>
    Task<object?> FindRaidByNameAndVersionAsync(string prefix, string suffix, int version);

    /// <summary>
    /// GET /raid/{prefix}/{suffix}/permissions
    /// Retrieve RAiD permissions.
    /// </summary>
    Task<RAiDPermissionsDto?> GetRaidPermissionsAsync(string prefix, string suffix);

    /// <summary>
    /// GET /raid/{prefix}/{suffix}/history
    /// Returns JSON patch change history for the RAiD.
    /// </summary>
    Task<List<RAiDChange>?> GetRaidHistoryAsync(string prefix, string suffix);

    #endregion

    #region Upgrade endpoints

    /// <summary>
    /// POST /upgrade
    /// Upgrades a RAiD, returning the updated RAiDDto.
    /// </summary>
    Task<RAiDDto?> UpgradeRaidAsync(RAiDUpdateRequest updateRequest);

    /// <summary>
    /// GET /upgradable/all
    /// Returns a list of RAiDs that are upgradable.
    /// </summary>
    Task<List<RAiDDto>?> FindAllUpgradableAsync();

    #endregion
}