using System.Net;
using System.Text;
using Moq;
using Moq.Protected;
using RAiD.Domain;
using Xunit;

namespace RAiD.Client.Tests;

public class RAiDApiClientTests
{
    [Fact]
    public async Task FindServicePointByIdAsync_ReturnsServicePoint_On200Response()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        const string servicePointJson = """
                                        {
                                            "id": 123,
                                            "name": "Test SP",
                                            "identifierOwner": "https://ror.org/abcde",
                                            "techEmail": "tech@test.org",
                                            "adminEmail": "admin@test.org",
                                            "enabled": true
                                        }
                                        """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.EndsWith("/service-point/123", request.RequestUri!.ToString());

                return new()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(servicePointJson, Encoding.UTF8, "application/json"),
                };
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };

        RAiDApiClient client = new(httpClient);

        // Act
        RAiDServicePoint? result = await client.FindServicePointByIdAsync(123);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(123, result.Id);
        Assert.Equal("Test SP", result.Name);
        Assert.True(result.Enabled);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task UpdateServicePointAsync_SendsPutRequestAndReturnsResult_OnSuccess()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        const string updatedJson = """
                                   {
                                       "id": 9999,
                                       "name": "Updated Name",
                                       "identifierOwner": "https://ror.org/xxx",
                                       "techEmail": "tech@update.org",
                                       "adminEmail": "admin@update.org",
                                       "enabled": false
                                   }
                                   """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                Assert.Equal(HttpMethod.Put, request.Method);
                Assert.EndsWith("/service-point/9999", request.RequestUri!.ToString());

                return new()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(updatedJson, Encoding.UTF8, "application/json"),
                };
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };

        RAiDApiClient client = new(httpClient);

        RAiDServicePointUpdateRequest updateRequest = new()
        {
            Id = 9999,
            Name = "Old Name", // The server responds with "Updated Name"
            IdentifierOwner = "https://ror.org/xxx",
            GroupId = "some-group-id",
        };

        // Act
        RAiDServicePoint? result = await client.UpdateServicePointAsync(9999, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(9999, result.Id);
        Assert.Equal("Updated Name", result.Name);
        Assert.False(result.Enabled);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task PatchRaidAsync_SendsPatchRequest_ReturnsUpdatedRaid()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        const string updatedRaidJson = """
                                       {
                                           "identifier": {
                                               "id": "https://raid.org/10.555/xyz",
                                               "schemaUri": "https://raid.org",
                                               "registrationAgency": {
                                                   "id": "https://ror.org/02stey378",
                                                   "schemaUri": "https://ror.org"
                                               },
                                               "owner": {
                                                   "id": "https://ror.org/02stey378",
                                                   "schemaUri": "https://ror.org"
                                               },
                                               "license": "Creative Commons CC-0",
                                               "version": 2
                                           },
                                           "access": {
                                               "type": {
                                                   "id": "https://vocabularies.coar-repositories.org/access_rights/c_abf2/",
                                                   "schemaUri": "https://vocabularies.coar-repositories.org/access_rights/"
                                               }
                                           }
                                       }
                                       """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                Assert.Equal("PATCH", request.Method.ToString());
                Assert.EndsWith("/raid/10.555/xyz", request.RequestUri!.ToString());

                return new()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(updatedRaidJson, Encoding.UTF8, "application/json"),
                };
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };
        RAiDApiClient client = new(httpClient);

        RAiDPatchRequest patchRequest = new()
        {
            Contributor =
            [
                new()
                {
                    Id = "some-orcid",
                    SchemaUri = "https://orcid.org/",
                    Position = [],
                    Role = [],
                },
            ],
        };

        // Act
        RAiDDto? result = await client.PatchRaidAsync("10.555", "xyz", patchRequest);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Identifier);
        Assert.Equal("https://raid.org/10.555/xyz", result.Identifier.IdValue);
        Assert.Equal(2, result.Identifier.Version);
    }

    [Fact]
    public async Task MintRaidAsync_ReturnsNewRaidDto_On201Response()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        const string mintedRaidJson = """
                                      {
                                          "identifier": {
                                              "id": "https://raid.org/10.1234/minted",
                                              "schemaUri": "https://raid.org",
                                              "registrationAgency": {
                                                  "id": "https://ror.org/something",
                                                  "schemaUri": "https://ror.org"
                                              },
                                              "owner": {
                                                  "id": "https://ror.org/02stey378",
                                                  "schemaUri": "https://ror.org"
                                              },
                                              "license": "Creative Commons CC-0",
                                              "version": 1
                                          },
                                          "access": {
                                              "type": {
                                                  "id": "https://vocabularies.coar-repositories.org/access_rights/c_abf2/",
                                                  "schemaUri": "https://vocabularies.coar-repositories.org/access_rights/"
                                              }
                                          }
                                      }
                                      """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.EndsWith("/raid/", request.RequestUri!.ToString());

                return new()
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent(mintedRaidJson, Encoding.UTF8, "application/json"),
                };
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };
        RAiDApiClient client = new(httpClient);

        RAiDCreateRequest createRequest = new()
        {
            // Only "access" is strictly required; fill in as needed
            Access = new()
            {
                Type = new()
                {
                    Id = "https://vocabularies.coar-repositories.org/access_rights/c_abf2/",
                    SchemaUri = "https://vocabularies.coar-repositories.org/access_rights/",
                },
            },
        };

        // Act
        RAiDDto? newRaid = await client.MintRaidAsync(createRequest);

        // Assert
        Assert.NotNull(newRaid);
        Assert.Equal("https://raid.org/10.1234/minted", newRaid.Identifier.IdValue);
        Assert.Equal(1, newRaid.Identifier.Version);
    }

    [Fact]
    public async Task FindAllServicePointsAsync_ReturnsList_On200Response()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        const string servicePointsJson = """
                                         [
                                             {
                                                 "id": 1,
                                                 "name": "SP1",
                                                 "identifierOwner": "https://ror.org/abc",
                                                 "techEmail": "tech1@example.org",
                                                 "adminEmail": "admin1@example.org",
                                                 "enabled": true
                                             },
                                             {
                                                 "id": 2,
                                                 "name": "SP2",
                                                 "identifierOwner": "https://ror.org/def",
                                                 "techEmail": "tech2@example.org",
                                                 "adminEmail": "admin2@example.org",
                                                 "enabled": false
                                             }
                                         ]
                                         """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.EndsWith("/service-point/", request.RequestUri!.ToString());

                return new()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(servicePointsJson, Encoding.UTF8, "application/json"),
                };
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };
        RAiDApiClient client = new(httpClient);

        // Act
        var spList = await client.FindAllServicePointsAsync();

        // Assert
        Assert.NotNull(spList);
        Assert.Equal(2, spList.Count);
        Assert.Equal("SP1", spList[0].Name);
        Assert.Equal("SP2", spList[1].Name);
    }

    [Fact]
    public async Task CreateServicePointAsync_ReturnsCreatedSP_On201Response()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        const string createdSpJson = """
                                     {
                                         "id": 42,
                                         "name": "Newly Created SP",
                                         "identifierOwner": "https://ror.org/new",
                                         "techEmail": "tech@newsp.org",
                                         "adminEmail": "admin@newsp.org",
                                         "enabled": true
                                     }
                                     """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.EndsWith("/service-point/", request.RequestUri!.ToString());

                return new()
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent(createdSpJson, Encoding.UTF8, "application/json"),
                };
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };
        RAiDApiClient client = new(httpClient);

        RAiDServicePointCreateRequest createReq = new()
        {
            Name = "Newly Created SP",
            IdentifierOwner = "https://ror.org/new",
            GroupId = "my-group-id",
            // other fields as needed
        };

        // Act
        RAiDServicePoint? createdSp = await client.CreateServicePointAsync(createReq);

        // Assert
        Assert.NotNull(createdSp);
        Assert.Equal(42, createdSp.Id);
        Assert.Equal("Newly Created SP", createdSp.Name);
    }

    [Fact]
    public async Task FindAllRaidsAsync_ReturnsRaids_On200Response()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        const string raidsJson = """
                                 [
                                     {
                                         "identifier": {
                                             "id": "https://raid.org/10.abc/123",
                                             "schemaUri": "https://raid.org",
                                             "license": "CC-0",
                                             "version": 1,
                                             "registrationAgency": {
                                                 "id": "https://ror.org/agency",
                                                 "schemaUri": "https://ror.org"
                                             },
                                             "owner": {
                                                 "id": "https://ror.org/org1",
                                                 "schemaUri": "https://ror.org"
                                             }
                                         },
                                         "access": {
                                             "type": {
                                                 "id": "https://vocabularies.coar-repositories.org/access_rights/c_abf2/",
                                                 "schemaUri": "https://vocabularies.coar-repositories.org/access_rights/"
                                             }
                                         }
                                     }
                                 ]
                                 """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.Contains("/raid", request.RequestUri!.ToString()); // Could check for query params
                return new()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(raidsJson, Encoding.UTF8, "application/json"),
                };
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };
        RAiDApiClient client = new(httpClient);

        // Act
        var raids = await client.FindAllRaidsAsync();

        // Assert
        Assert.NotNull(raids);
        Assert.Single(raids);
        Assert.Equal("https://raid.org/10.abc/123", raids[0].Identifier.IdValue);
    }

    [Fact]
    public async Task MintRaidAsync_ReturnsNewRaid_On201Response()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        const string mintedRaidJson = """
                                      {
                                          "identifier": {
                                              "id": "https://raid.org/10.9999/minted",
                                              "schemaUri": "https://raid.org",
                                              "license": "CC-0",
                                              "version": 1,
                                              "registrationAgency": {
                                                  "id": "https://ror.org/agency",
                                                  "schemaUri": "https://ror.org"
                                              },
                                              "owner": {
                                                  "id": "https://ror.org/someOrg",
                                                  "schemaUri": "https://ror.org"
                                              }
                                          },
                                          "access": {
                                              "type": {
                                                  "id": "https://vocabularies.coar-repositories.org/access_rights/c_abf2/",
                                                  "schemaUri": "https://vocabularies.coar-repositories.org/access_rights/"
                                              }
                                          }
                                      }
                                      """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.EndsWith("/raid/", request.RequestUri!.ToString());
                return new()
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent(mintedRaidJson, Encoding.UTF8, "application/json"),
                };
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };
        RAiDApiClient client = new(httpClient);

        RAiDCreateRequest createRequest = new()
        {
            // At least "access" is required
            Access = new()
            {
                Type = new()
                {
                    Id = "https://vocabularies.coar-repositories.org/access_rights/c_abf2/",
                    SchemaUri = "https://vocabularies.coar-repositories.org/access_rights/",
                },
            },
        };

        // Act
        RAiDDto? newRaid = await client.MintRaidAsync(createRequest);

        // Assert
        Assert.NotNull(newRaid);
        Assert.Equal("https://raid.org/10.9999/minted", newRaid.Identifier.IdValue);
        Assert.Equal(1, newRaid.Identifier.Version);
    }

    [Fact]
    public async Task FindRaidByNameAsync_ReturnsRaidDto_On200Response()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        const string raidJson = """
                                {
                                    "identifier": {
                                        "id": "https://raid.org/10.1234/abcd",
                                        "schemaUri": "https://raid.org",
                                        "license": "CC-0",
                                        "version": 1,
                                        "registrationAgency": {
                                            "id": "https://ror.org/agency",
                                            "schemaUri": "https://ror.org"
                                        },
                                        "owner": {
                                            "id": "https://ror.org/orgX",
                                            "schemaUri": "https://ror.org"
                                        }
                                    },
                                    "access": {
                                        "type": {
                                            "id": "https://vocabularies.coar-repositories.org/access_rights/c_abf2/",
                                            "schemaUri": "https://vocabularies.coar-repositories.org/access_rights/"
                                        }
                                    }
                                }
                                """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.EndsWith("/raid/prefix/suffix", request.RequestUri!.ToString());
                return new()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(raidJson, Encoding.UTF8, "application/json"),
                };
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };
        RAiDApiClient client = new(httpClient);

        // Act
        RAiDDto? raid = await client.FindRaidByNameAsync("prefix", "suffix");

        // Assert
        Assert.NotNull(raid);
        Assert.Equal("https://raid.org/10.1234/abcd", raid.Identifier.IdValue);
        Assert.Equal(1, raid.Identifier.Version);
    }

    [Fact]
    public async Task PatchRaidAsync_PatchesRaid_ReturnsUpdatedRaid()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        const string patchedRaidJson = """
                                       {
                                           "identifier": {
                                               "id": "https://raid.org/10.555/xyz",
                                               "schemaUri": "https://raid.org",
                                               "license": "CC-0",
                                               "version": 2,
                                               "registrationAgency": {
                                                   "id": "https://ror.org/agency",
                                                   "schemaUri": "https://ror.org"
                                               },
                                               "owner": {
                                                   "id": "https://ror.org/orgX",
                                                   "schemaUri": "https://ror.org"
                                               }
                                           },
                                           "access": {
                                               "type": {
                                                   "id": "https://vocabularies.coar-repositories.org/access_rights/c_abf2/",
                                                   "schemaUri": "https://vocabularies.coar-repositories.org/access_rights/"
                                               }
                                           }
                                       }
                                       """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                Assert.Equal("PATCH", request.Method.ToString());
                Assert.EndsWith("/raid/pfx/sfx", request.RequestUri!.ToString());
                return new()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(patchedRaidJson, Encoding.UTF8, "application/json"),
                };
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };
        RAiDApiClient client = new(httpClient);

        RAiDPatchRequest patchReq = new()
        {
            Contributor = new()
            {
                new()
                {
                    Id = "some-orcid-id",
                    SchemaUri = "https://orcid.org/",
                    Position = new(),
                    Role = new(),
                },
            },
        };

        // Act
        RAiDDto? updatedRaid = await client.PatchRaidAsync("pfx", "sfx", patchReq);

        // Assert
        Assert.NotNull(updatedRaid);
        Assert.Equal("https://raid.org/10.555/xyz", updatedRaid.Identifier.IdValue);
        Assert.Equal(2, updatedRaid.Identifier.Version);
    }

    [Fact]
    public async Task FindAllPublicRaidsAsync_ReturnsPublicRaids_On200Response()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        const string publicRaidsJson = """
                                       [
                                           {
                                               "identifier": {
                                                   "id": "https://raid.org/10.public/111",
                                                   "version": 1,
                                                   "schemaUri": "https://raid.org",
                                                   "registrationAgency": {
                                                       "id": "https://ror.org/someagency",
                                                       "schemaUri": "https://ror.org"
                                                   },
                                                   "owner": {
                                                       "id": "https://ror.org/001",
                                                       "schemaUri": "https://ror.org"
                                                   },
                                                   "license": "CC-0"
                                               },
                                               "access": {
                                                   "type": {
                                                       "id": "https://vocabularies.coar-repositories.org/access_rights/c_abf2/",
                                                       "schemaUri": "https://vocabularies.coar-repositories.org/access_rights/"
                                                   }
                                               }
                                           }
                                       ]
                                       """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(publicRaidsJson, Encoding.UTF8, "application/json"),
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };
        RAiDApiClient client = new(httpClient);

        // Act
        var list = await client.FindAllPublicRaidsAsync();

        // Assert
        Assert.NotNull(list);
        Assert.Single(list);
        Assert.Equal("https://raid.org/10.public/111", list[0].Identifier.IdValue);
    }

    [Fact]
    public async Task FindRaidByNameAndVersionAsync_ReturnsObject_On200Response()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        const string genericObjectJson = """
                                         {
                                             "someField": "someValue",
                                             "nested": {
                                                 "anotherField": 123
                                             }
                                         }
                                         """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.EndsWith("/raid/prefix/suffix/3", request.RequestUri!.ToString());
                return new()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(genericObjectJson, Encoding.UTF8, "application/json"),
                };
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };
        RAiDApiClient client = new(httpClient);

        // Act
        object? rawObject = await client.FindRaidByNameAndVersionAsync("prefix", "suffix", 3);

        // Assert
        // The method returns 'object?', so we simply check it's not null.
        // If you expect a certain shape, you can cast or parse further.
        Assert.NotNull(rawObject);
    }

    [Fact]
    public async Task GetRaidPermissionsAsync_ReturnsPermissions_On200Response()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        const string permissionsJson = """
                                       {
                                           "servicePointMatch": true,
                                           "read": true,
                                           "write": false
                                       }
                                       """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.EndsWith("/raid/prefix/suffix/permissions", request.RequestUri!.ToString());
                return new()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(permissionsJson, Encoding.UTF8, "application/json"),
                };
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };
        RAiDApiClient client = new(httpClient);

        // Act
        RAiDPermissionsDto? perms = await client.GetRaidPermissionsAsync("prefix", "suffix");

        // Assert
        Assert.NotNull(perms);
        Assert.True(perms.ServicePointMatch);
        Assert.True(perms.Read);
        Assert.False(perms.Write);
    }

    [Fact]
    public async Task GetRaidHistoryAsync_ReturnsHistoryList_On200Response()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        const string historyJson = """
                                   [
                                       {
                                           "handle": "10.25.1.1/abcde",
                                           "version": 1,
                                           "diff": "base64EncodedPatch",
                                           "timestamp": "2024-01-01T01:23:45Z"
                                       },
                                       {
                                           "handle": "10.25.1.1/abcde",
                                           "version": 2,
                                           "diff": "anotherBase64EncodedPatch",
                                           "timestamp": "2024-02-02T12:00:00Z"
                                       }
                                   ]
                                   """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.EndsWith("/raid/prefix/suffix/history", request.RequestUri!.ToString());
                return new()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(historyJson, Encoding.UTF8, "application/json"),
                };
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };
        RAiDApiClient client = new(httpClient);

        // Act
        var history = await client.GetRaidHistoryAsync("prefix", "suffix");

        // Assert
        Assert.NotNull(history);
        Assert.Equal(2, history.Count);
        Assert.Equal(1, history[0].Version);
        Assert.Equal("base64EncodedPatch", history[0].Diff);
    }

    [Fact]
    public async Task UpgradeRaidAsync_ReturnsUpdatedRaid_On200Response()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        const string updatedRaidJson = """
                                       {
                                           "identifier": {
                                               "id": "https://raid.org/10.some/upgrade",
                                               "schemaUri": "https://raid.org",
                                               "license": "CC-0",
                                               "version": 2,
                                               "registrationAgency": {
                                                   "id": "https://ror.org/agency",
                                                   "schemaUri": "https://ror.org"
                                               },
                                               "owner": {
                                                   "id": "https://ror.org/orgX",
                                                   "schemaUri": "https://ror.org"
                                               }
                                           },
                                           "access": {
                                               "type": {
                                                   "id": "https://vocabularies.coar-repositories.org/access_rights/c_abf2/",
                                                   "schemaUri": "https://vocabularies.coar-repositories.org/access_rights/"
                                               }
                                           }
                                       }
                                       """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.EndsWith("/upgrade", request.RequestUri!.ToString());
                return new()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(updatedRaidJson, Encoding.UTF8, "application/json"),
                };
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };
        RAiDApiClient client = new(httpClient);

        RAiDUpdateRequest updateReq = new()
        {
            Access = new()
            {
                Type = new()
                {
                    Id = "https://vocabularies.coar-repositories.org/access_rights/c_abf2/",
                    SchemaUri = "https://vocabularies.coar-repositories.org/access_rights/",
                },
            },
            Identifier = new()
            {
                IdValue = "https://raid.org/10.some/upgrade",
                SchemaUri = "https://raid.org",
                License = "CC-0",
                Version = 1,
                RegistrationAgency = new()
                {
                    Id = "https://ror.org/agency",
                    SchemaUri = "https://ror.org",
                },
                Owner = new()
                {
                    Id = "https://ror.org/orgX",
                    SchemaUri = "https://ror.org",
                },
            },
        };

        // Act
        RAiDDto? upgradedRaid = await client.UpgradeRaidAsync(updateReq);

        // Assert
        Assert.NotNull(upgradedRaid);
        Assert.Equal("https://raid.org/10.some/upgrade", upgradedRaid.Identifier.IdValue);
        Assert.Equal(2, upgradedRaid.Identifier.Version);
    }

    [Fact]
    public async Task FindAllUpgradableAsync_ReturnsUpgradableRaids_On200Response()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        const string upgradableRaidsJson = """
                                           [
                                               {
                                                   "identifier": {
                                                       "id": "https://raid.org/10.up/1234",
                                                       "version": 1,
                                                       "schemaUri": "https://raid.org",
                                                       "license": "CC-0",
                                                       "registrationAgency": {
                                                           "id": "https://ror.org/agency",
                                                           "schemaUri": "https://ror.org"
                                                       },
                                                       "owner": {
                                                           "id": "https://ror.org/org1",
                                                           "schemaUri": "https://ror.org"
                                                       }
                                                   },
                                                   "access": {
                                                       "type": {
                                                           "id": "https://vocabularies.coar-repositories.org/access_rights/c_abf2/",
                                                           "schemaUri": "https://vocabularies.coar-repositories.org/access_rights/"
                                                       }
                                                   }
                                               },
                                               {
                                                   "identifier": {
                                                       "id": "https://raid.org/10.up/9999",
                                                       "version": 2,
                                                       "schemaUri": "https://raid.org",
                                                       "license": "CC-0",
                                                       "registrationAgency": {
                                                           "id": "https://ror.org/agency",
                                                           "schemaUri": "https://ror.org"
                                                       },
                                                       "owner": {
                                                           "id": "https://ror.org/org2",
                                                           "schemaUri": "https://ror.org"
                                                       }
                                                   },
                                                   "access": {
                                                       "type": {
                                                           "id": "https://vocabularies.coar-repositories.org/access_rights/c_abf2/",
                                                           "schemaUri": "https://vocabularies.coar-repositories.org/access_rights/"
                                                       }
                                                   }
                                               }
                                           ]
                                           """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.EndsWith("/upgradable/all", request.RequestUri!.ToString());
                return new()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(upgradableRaidsJson, Encoding.UTF8, "application/json"),
                };
            });

        HttpClient httpClient = new(handlerMock.Object)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };
        RAiDApiClient client = new(httpClient);

        // Act
        var upgradable = await client.FindAllUpgradableAsync();

        // Assert
        Assert.NotNull(upgradable);
        Assert.Equal(2, upgradable.Count);
        Assert.Equal("https://raid.org/10.up/1234", upgradable[0].Identifier.IdValue);
        Assert.Equal("https://raid.org/10.up/9999", upgradable[1].Identifier.IdValue);
    }
}
