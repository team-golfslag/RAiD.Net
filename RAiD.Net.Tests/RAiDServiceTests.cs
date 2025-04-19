using System.Collections;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using RAiD.Net.Domain;
using Xunit;

namespace RAiD.Net.Tests;

public class RAiDServiceTests
{
    private static RAiDService CreateService(HttpMessageHandler handler)
    {
        HttpClient httpClient = new(handler)
        {
            BaseAddress = new("https://fake-raid.local/"),
        };

        var optionsMock = new Mock<IOptions<RAiDServiceOptions>>();
        optionsMock
            .Setup(o => o.Value)
            .Returns(new RAiDServiceOptions
            {
                BaseUrl = "https://fake-raid.local/",
            });

        var loggerMock = new Mock<ILogger<RAiDService>>();

        return new(httpClient, optionsMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task FindServicePointByIdAsync_ReturnsServicePoint_On200Response()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        const string json = """
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
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                ArgumentNullException.ThrowIfNull(req);
                Assert.Equal(HttpMethod.Get, req.Method);
                Assert.EndsWith("/service-point/123", req.RequestUri!.ToString());
                return new()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };
            });

        RAiDService service = CreateService(handlerMock.Object);

        RAiDServicePoint? sp = await service.FindServicePointByIdAsync(123);

        Assert.NotNull(sp);
        Assert.Equal(123, sp.Id);
        Assert.Equal("Test SP", sp.Name);
        Assert.True(sp.Enabled);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task UpdateServicePointAsync_ReturnsUpdatedServicePoint_On200Response()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        const string json = """
                            {
                              "id": 9999,
                              "name": "Updated",
                              "identifierOwner": "https://ror.org/xxx",
                              "techEmail": "tech@u.org",
                              "adminEmail": "admin@u.org",
                              "enabled": false
                            }
                            """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                ArgumentNullException.ThrowIfNull(req);
                Assert.Equal(HttpMethod.Put, req.Method);
                Assert.EndsWith("/service-point/9999", req.RequestUri!.ToString());
                return new()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };
            });

        RAiDService service = CreateService(handlerMock.Object);
        RAiDServicePointUpdateRequest updateReq = new()
        {
            Id = 9999,
            Name = "Will be ignored",
            IdentifierOwner = "https://ror.org/xxx",
            GroupId = "gid",
        };

        RAiDServicePoint? sp = await service.UpdateServicePointAsync(9999, updateReq);

        Assert.NotNull(sp);
        Assert.Equal(9999, sp.Id);
        Assert.Equal("Updated", sp.Name);
        Assert.False(sp.Enabled);
    }

    [Fact]
    public async Task FindAllServicePointsAsync_ReturnsList_On200Response()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        const string json = """
                            [
                              { "id":1,"name":"SP1","identifierOwner":"x","techEmail":"","adminEmail":"","enabled":true },
                              { "id":2,"name":"SP2","identifierOwner":"y","techEmail":"","adminEmail":"","enabled":false }
                            ]
                            """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });

        RAiDService service = CreateService(handlerMock.Object);

        var list = await service.FindAllServicePointsAsync();

        Assert.NotNull(list);
        Assert.Equal(2, list.Count);
        Assert.Equal("SP1", list[0].Name);
        Assert.Equal("SP2", list[1].Name);
    }

    [Fact]
    public async Task CreateServicePointAsync_ReturnsCreatedSP_On201Response()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        const string json = """
                            {
                              "id": 42,
                              "name": "New SP",
                              "identifierOwner": "https://ror.org/new",
                              "techEmail": "t@new","adminEmail":"a@new","enabled":true
                            }
                            """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });

        RAiDService service = CreateService(handlerMock.Object);
        RAiDServicePointCreateRequest createReq = new()
        {
            Name = "New SP",
            IdentifierOwner = "https://ror.org/new",
            GroupId = "g",
        };

        RAiDServicePoint? sp = await service.CreateServicePointAsync(createReq);

        Assert.NotNull(sp);
        Assert.Equal(42, sp.Id);
        Assert.Equal("New SP", sp.Name);
    }

    [Fact]
    public async Task FindRaidByNameAsync_ReturnsDto_On200Response()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        const string json = """
                            {
                              "identifier": { "id":"https://raid.org/p/s","schemaUri":"u","license":"L","version":1,
                                             "registrationAgency":{ "id":"r","schemaUri":"u" },
                                             "owner":{ "id":"o","schemaUri":"u" } },
                              "access": { "type":{ "id":"x","schemaUri":"u" } }
                            }
                            """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                ArgumentNullException.ThrowIfNull(req);
                Assert.EndsWith("/raid/prefix/suffix", req.RequestUri!.ToString());
                return new()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };
            });

        RAiDService service = CreateService(handlerMock.Object);
        RAiDDto? dto = await service.FindRaidByNameAsync("prefix", "suffix");

        Assert.NotNull(dto);
        Assert.Equal("https://raid.org/p/s", dto.Identifier.IdValue);
        Assert.Equal(1, dto.Identifier.Version);
    }

    [Fact]
    public async Task UpdateRaidAsync_ReturnsDto_On200Response()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        const string json = """
                            {
                              "identifier": { "id":"i","schemaUri":"u","license":"L","version":2,
                                             "registrationAgency":{ "id":"r","schemaUri":"u" },
                                             "owner":{ "id":"o","schemaUri":"u" } },
                              "access": { "type":{ "id":"x","schemaUri":"u" } }
                            }
                            """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });

        RAiDService service = CreateService(handlerMock.Object);
        RAiDUpdateRequest req = new()
        {
            Identifier = new()
            {
                IdValue = "i",
                SchemaUri = "u",
                License = "L",
                Version = 1,
                RegistrationAgency = new() { Id="r", SchemaUri="u" },
                Owner = new() { Id="o", SchemaUri="u" },
            },
            Access = new()
            {
                Type = new() { Id="x", SchemaUri="u" },
            },
        };

        RAiDDto? dto = await service.UpdateRaidAsync("p","s", req);

        Assert.NotNull(dto);
        Assert.Equal(2, dto.Identifier.Version);
    }

    [Fact]
    public async Task PatchRaidAsync_ReturnsDto_On200Response()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        const string json = """
                            {
                              "identifier": { "id":"i","schemaUri":"u","license":"L","version":3,
                                             "registrationAgency":{ "id":"r","schemaUri":"u" },
                                             "owner":{ "id":"o","schemaUri":"u" } },
                              "access": { "type":{ "id":"x","schemaUri":"u" } }
                            }
                            """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
            {
                ArgumentNullException.ThrowIfNull(req);
                Assert.Equal("PATCH", req.Method.Method);
                Assert.EndsWith("/raid/p/s", req.RequestUri!.ToString());
                return new()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };
            });

        RAiDService service = CreateService(handlerMock.Object);
        RAiDPatchRequest patchReq = new()
            { /* set fields if needed */ };

        RAiDDto? dto = await service.PatchRaidAsync("p","s", patchReq);

        Assert.NotNull(dto);
        Assert.Equal(3, dto.Identifier.Version);
    }

    [Fact]
    public async Task FindAllRaidsAsync_ReturnsList_On200Response()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        const string json = """
                            [
                              {
                                "identifier": { "id":"i","schemaUri":"u","license":"L","version":1,
                                               "registrationAgency":{ "id":"r","schemaUri":"u" },
                                               "owner":{ "id":"o","schemaUri":"u" } },
                                "access": { "type":{ "id":"x","schemaUri":"u" } }
                              }
                            ]
                            """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });

        RAiDService service = CreateService(handlerMock.Object);
        var list = await service.FindAllRaidsAsync();

        Assert.NotNull(list);
        Assert.Single((IEnumerable)list);
        Assert.Equal("i", list[0].Identifier.IdValue);
    }

    [Fact]
    public async Task MintRaidAsync_ReturnsDto_On201Response()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        const string json = """
                            {
                              "identifier": { "id":"m","schemaUri":"u","license":"L","version":1,
                                             "registrationAgency":{ "id":"r","schemaUri":"u" },
                                             "owner":{ "id":"o","schemaUri":"u" } },
                              "access": { "type":{ "id":"x","schemaUri":"u" } }
                            }
                            """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });

        RAiDService service = CreateService(handlerMock.Object);
        RAiDCreateRequest createReq = new()
        {
            Access = new()
            {
                Type = new() { Id="x", SchemaUri="u" },
            },
        };

        RAiDDto? dto = await service.MintRaidAsync(createReq);

        Assert.NotNull(dto);
        Assert.Equal(1, dto.Identifier.Version);
    }

    [Fact]
    public async Task FindAllPublicRaidsAsync_ReturnsList_On200Response()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        const string json = """
                            [
                              {
                                "identifier": { "id":"pub","schemaUri":"u","license":"L","version":1,
                                               "registrationAgency":{ "id":"r","schemaUri":"u" },
                                               "owner":{ "id":"o","schemaUri":"u" } },
                                "access": { "type":{ "id":"x","schemaUri":"u" } }
                              }
                            ]
                            """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });

        RAiDService service = CreateService(handlerMock.Object);
        var list = await service.FindAllPublicRaidsAsync();

        Assert.NotNull(list);
        Assert.Single((IEnumerable)list);
        Assert.Equal("pub", list[0].Identifier.IdValue);
    }

    [Fact]
    public async Task FindRaidByNameAndVersionAsync_ReturnsObject_On200Response()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        const string json = """ { "foo":"bar" } """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });

        RAiDService service = CreateService(handlerMock.Object);
        object? obj = await service.FindRaidByNameAndVersionAsync("p","s",5);

        Assert.NotNull(obj);
    }

    [Fact]
    public async Task GetRaidPermissionsAsync_ReturnsDto_On200Response()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        const string json = """
                            { "servicePointMatch":true,"read":true,"write":false }
                            """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });

        RAiDService service = CreateService(handlerMock.Object);
        RAiDPermissionsDto? perms = await service.GetRaidPermissionsAsync("p","s");

        Assert.NotNull(perms);
        Assert.True(perms.ServicePointMatch);
        Assert.True(perms.Read);
        Assert.False(perms.Write);
    }

    [Fact]
    public async Task GetRaidHistoryAsync_ReturnsList_On200Response()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        const string json = """
                            [
                              { "handle":"h","version":1,"diff":"d","timestamp":"2024-01-01T00:00:00Z" }
                            ]
                            """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });

        RAiDService service = CreateService(handlerMock.Object);
        var history = await service.GetRaidHistoryAsync("p","s");

        Assert.NotNull(history);
        Assert.Single((IEnumerable)history);
        Assert.Equal(1, history[0].Version);
    }

    [Fact]
    public async Task UpgradeRaidAsync_ReturnsDto_On200Response()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        const string json = """
                            {
                              "identifier": { "id":"u","schemaUri":"u","license":"L","version":2,
                                             "registrationAgency":{ "id":"r","schemaUri":"u" },
                                             "owner":{ "id":"o","schemaUri":"u" } },
                              "access": { "type":{ "id":"x","schemaUri":"u" } }
                            }
                            """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });

        RAiDService service = CreateService(handlerMock.Object);
        RAiDUpdateRequest req = new()
        {
            Identifier = new()
            {
                IdValue = "u",
                SchemaUri = "u",
                License = "L",
                Version = 1,
                RegistrationAgency = new() { Id="r", SchemaUri="u" },
                Owner = new() { Id="o", SchemaUri="u" },
            },
            Access = new()
            {
                Type = new() { Id="x", SchemaUri="u" },
            },
        };

        RAiDDto? dto = await service.UpgradeRaidAsync(req);

        Assert.NotNull(dto);
        Assert.Equal(2, dto.Identifier.Version);
    }

    [Fact]
    public async Task FindAllUpgradableAsync_ReturnsList_On200Response()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        const string json = """
                            [
                              { "identifier":{ "id":"a","schemaUri":"u","license":"L","version":1,
                                              "registrationAgency":{ "id":"r","schemaUri":"u" },
                                              "owner":{ "id":"o","schemaUri":"u" } },
                                "access":{ "type":{ "id":"x","schemaUri":"u" } }
                              }
                            ]
                            """;

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });

        RAiDService service = CreateService(handlerMock.Object);
        var list = await service.FindAllUpgradableAsync();

        Assert.NotNull(list);
        Assert.Single((IEnumerable)list);
        Assert.Equal("a", list[0].Identifier.IdValue);
    }
}