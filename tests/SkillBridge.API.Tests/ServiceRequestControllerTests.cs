using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using SoftBridge.Abstraction.IServices.Profiles;
using SoftBridge.Abstraction.IServicesContract.Request;
using SoftBridge.Shared.Common.Dto.Client;
using SoftBridge.Shared.Common.Dto.ServiceProvider;
using SoftBridge.Shared.Common.Dto.ServiceRequest.NewDtos;
using SoftBridge.Shared.Common.Pagination;
using SoftBridge.Shared.Common.Params.Requests;
using SoftBridge.Web;
using Xunit;

namespace SoftBridge.API.Tests.Controllers;

/// <summary>
/// Integration tests for RequestController.
/// Routes:
///   POST   api/request
///   GET    api/request/client
///   GET    api/request/provider
///   PUT    api/request/{id}/accept
///   PUT    api/request/{id}/reject
///   GET    api/request/{id}
///   PUT    api/request/{id}/complete
///   GET    api/request/admin
/// </summary>
public class ServiceRequestControllerTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly Mock<IRequestWorkflowService> _requestServiceMock;
    private readonly Mock<IClientProfileService> _clientProfileServiceMock;
    private readonly Mock<IProviderProfileService> _providerProfileServiceMock;

    // JWT settings must match appsettings.json
    private const string JwtKey = "7zzJSeUi5eL3PfoQt7XieaSsbzIZ3CS5juJNbW4kouJx6Bpdkt";
    private const string JwtIssuer = "http://localhost:5000";
    private const string JwtAudience = "SoftBridge";

    private const string BaseRoute = "api/request";

    private const string ClientUserId = "service-request-client-user";
    private const string ProviderUserId = "service-request-provider-user";

    private static readonly string DbName = "ServiceRequestTestDb_" + Guid.NewGuid();
    private static readonly Guid TestClientId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TestProviderId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public ServiceRequestControllerTests(WebApplicationFactory<Program> factory)
    {
        _requestServiceMock = new Mock<IRequestWorkflowService>();
        _clientProfileServiceMock = new Mock<IClientProfileService>();
        _providerProfileServiceMock = new Mock<IProviderProfileService>();

        _clientProfileServiceMock
            .Setup(s => s.GetMyProfileAsync(It.IsAny<string>()))
            .ReturnsAsync((string userId) => new ClientProfileDto
            {
                Id = TestClientId,
                Name = "Test Client",
                Email = $"{userId}@example.com",
                ProfileImageUrl = "https://cdn.example.com/client.png",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            });

        _providerProfileServiceMock
            .Setup(s => s.GetMyProfileAsync(It.IsAny<string>()))
            .ReturnsAsync((string userId) => new ProviderProfileDto
            {
                Id = TestProviderId,
                FullName = "Test Provider",
                Email = $"{userId}@example.com",
                AccountStatus = "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            });

        _requestServiceMock
            .Setup(s => s.SendServiceRequestAsync(It.IsAny<CreateRequestDto>(), It.IsAny<Guid>()))
            .ReturnsAsync((CreateRequestDto dto, Guid clientId) => new RequestDto
            {
                Id = Guid.NewGuid(),
                ServiceId = dto.ServiceId,
                ServiceTitle = "Plumbing Service",
                ClientId = clientId,
                ClientName = "Test Client",
                ProviderId = TestProviderId,
                ProviderName = "Test Provider",
                Message = dto.Message,
                AgreedPrice = dto.AgreedPrice,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });

        _requestServiceMock
            .Setup(s => s.GetClientRequestsAsync(It.IsAny<Guid>(), It.IsAny<RequestQueryParams>()))
            .ReturnsAsync((Guid _, RequestQueryParams query) =>
                new PaginationResponse<RequestDto>(
                    query.PageIndex,
                    query.PageSize,
                    1,
                    new List<RequestDto>
                    {
                        BuildRequestDto(status: "Pending")
                    }));

        _requestServiceMock
            .Setup(s => s.GetProviderRequestsAsync(It.IsAny<Guid>(), It.IsAny<RequestQueryParams>()))
            .ReturnsAsync((Guid _, RequestQueryParams query) =>
                new PaginationResponse<RequestDto>(
                    query.PageIndex,
                    query.PageSize,
                    1,
                    new List<RequestDto>
                    {
                        BuildRequestDto(status: "Pending")
                    }));

        _requestServiceMock
            .Setup(s => s.AcceptRequestAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(true);

        _requestServiceMock
            .Setup(s => s.RejectRequestAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _requestServiceMock
            .Setup(s => s.CompleteRequestAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(true);

        _requestServiceMock
            .Setup(s => s.GetRequestDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid requestId) => new RequestDetailsDto
            {
                Id = requestId,
                ServiceId = Guid.NewGuid(),
                ServiceTitle = "Plumbing Service",
                ServiceCategory = "Home Services",
                ServicePrice = 150,
                ClientId = TestClientId,
                ClientName = "Test Client",
                ClientEmail = "client@example.com",
                ProviderId = TestProviderId,
                ProviderName = "Test Provider",
                ProviderEmail = "provider@example.com",
                Message = "Need this done today",
                AgreedPrice = 120,
                Status = "Accepted",
                CreatedAt = DateTime.UtcNow.AddHours(-3),
                AcceptedAt = DateTime.UtcNow.AddHours(-2)
            });

        _requestServiceMock
            .Setup(s => s.GetAllPlatformRequestsAsync(It.IsAny<RequestQueryParams>()))
            .ReturnsAsync((RequestQueryParams query) =>
                new PaginationResponse<RequestDto>(
                    query.PageIndex,
                    query.PageSize,
                    2,
                    new List<RequestDto>
                    {
                        BuildRequestDto(status: "Pending"),
                        BuildRequestDto(status: "Accepted")
                    }));

        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtTokenSettings:Key"] = JwtKey,
                    ["JwtTokenSettings:SecretKey"] = JwtKey,
                    ["JwtTokenSettings:Issuer"] = JwtIssuer,
                    ["JwtTokenSettings:Audience"] = JwtAudience,
                    ["TestDbName"] = DbName,
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IRequestWorkflowService>();
                services.AddSingleton(_requestServiceMock.Object);

                services.RemoveAll<IClientProfileService>();
                services.AddSingleton(_clientProfileServiceMock.Object);

                services.RemoveAll<IProviderProfileService>();
                services.AddSingleton(_providerProfileServiceMock.Object);
            });
        }).CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    // ════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════════════════════

    private static RequestDto BuildRequestDto(string status) => new()
    {
        Id = Guid.NewGuid(),
        ServiceId = Guid.NewGuid(),
        ServiceTitle = "Plumbing Service",
        ClientId = TestClientId,
        ClientName = "Test Client",
        ProviderId = TestProviderId,
        ProviderName = "Test Provider",
        Message = "Need help ASAP",
        AgreedPrice = 100,
        Status = status,
        CreatedAt = DateTime.UtcNow
    };

    private static string GenerateJwt(string role, string? userId = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId ?? Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private void SetClientToken(string? userId = null) => SetToken("Client", userId ?? ClientUserId);
    private void SetProviderToken(string? userId = null) => SetToken("Provider", userId ?? ProviderUserId);
    private void SetAdminToken(string? userId = null) => SetToken("Admin", userId);
    private void ClearToken() => _client.DefaultRequestHeaders.Authorization = null;

    private void SetToken(string role, string? userId = null) =>
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateJwt(role, userId));

    private static StringContent ToJson(object obj) =>
        new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    // ════════════════════════════════════════════════════════════════════════
    // CLIENT ENDPOINTS
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SR01_SendRequest_ClientRole_Returns201()
    {
        SetClientToken(ClientUserId);
        var payload = new CreateRequestDto
        {
            ServiceId = Guid.NewGuid(),
            Message = "Need this service quickly",
            AgreedPrice = 125
        };

        var response = await _client.PostAsJsonAsync(BaseRoute, payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
        Assert.Equal("Need this service quickly", body.GetProperty("data").GetProperty("message").GetString());

        _clientProfileServiceMock.Verify(s => s.GetMyProfileAsync(ClientUserId), Times.Once);
        _requestServiceMock.Verify(
            s => s.SendServiceRequestAsync(
                It.Is<CreateRequestDto>(d =>
                    d.ServiceId == payload.ServiceId &&
                    d.Message == payload.Message &&
                    d.AgreedPrice == payload.AgreedPrice),
                TestClientId),
            Times.Once);
    }

    [Fact]
    public async Task SR02_SendRequest_NoToken_Returns401()
    {
        ClearToken();
        var payload = new CreateRequestDto
        {
            ServiceId = Guid.NewGuid(),
            Message = "No token request"
        };

        var response = await _client.PostAsJsonAsync(BaseRoute, payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SR03_SendRequest_ProviderRole_Returns403()
    {
        SetProviderToken();
        var payload = new CreateRequestDto
        {
            ServiceId = Guid.NewGuid(),
            Message = "Provider not allowed"
        };

        var response = await _client.PostAsJsonAsync(BaseRoute, payload);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SR04_GetClientRequests_ClientRole_Returns200()
    {
        SetClientToken(ClientUserId);

        var response = await _client.GetAsync($"{BaseRoute}/client?pageIndex=2&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(2, body.GetProperty("data").GetProperty("pageIndex").GetInt32());
        Assert.Equal(5, body.GetProperty("data").GetProperty("pageSize").GetInt32());

        _requestServiceMock.Verify(
            s => s.GetClientRequestsAsync(
                TestClientId,
                It.Is<RequestQueryParams>(q => q.PageIndex == 2 && q.PageSize == 5)),
            Times.Once);
    }

    [Fact]
    public async Task SR05_GetClientRequests_NoToken_Returns401()
    {
        ClearToken();
        var response = await _client.GetAsync($"{BaseRoute}/client");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // PROVIDER ENDPOINTS
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SR06_GetProviderRequests_ProviderRole_Returns200()
    {
        SetProviderToken(ProviderUserId);

        var response = await _client.GetAsync($"{BaseRoute}/provider?pageIndex=1&pageSize=3");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(3, body.GetProperty("data").GetProperty("pageSize").GetInt32());

        _providerProfileServiceMock.Verify(s => s.GetMyProfileAsync(ProviderUserId), Times.Once);
        _requestServiceMock.Verify(
            s => s.GetProviderRequestsAsync(
                TestProviderId,
                It.Is<RequestQueryParams>(q => q.PageIndex == 1 && q.PageSize == 3)),
            Times.Once);
    }

    [Fact]
    public async Task SR07_GetProviderRequests_ClientRole_Returns403()
    {
        SetClientToken();
        var response = await _client.GetAsync($"{BaseRoute}/provider");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SR08_AcceptRequest_ProviderRole_Returns200()
    {
        SetProviderToken(ProviderUserId);
        var requestId = Guid.NewGuid();

        var response = await _client.PutAsync(
            $"{BaseRoute}/{requestId}/accept",
            new StringContent(string.Empty, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _requestServiceMock.Verify(s => s.AcceptRequestAsync(requestId, TestProviderId), Times.Once);
    }

    [Fact]
    public async Task SR09_AcceptRequest_NoToken_Returns401()
    {
        ClearToken();
        var requestId = Guid.NewGuid();

        var response = await _client.PutAsync(
            $"{BaseRoute}/{requestId}/accept",
            new StringContent(string.Empty, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SR10_RejectRequest_ProviderRole_Returns200()
    {
        SetProviderToken(ProviderUserId);
        var requestId = Guid.NewGuid();
        const string rejectionReason = "Not available this week";

        var response = await _client.PutAsync(
            $"{BaseRoute}/{requestId}/reject",
            ToJson(rejectionReason));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _requestServiceMock.Verify(
            s => s.RejectRequestAsync(requestId, TestProviderId, rejectionReason),
            Times.Once);
    }

    [Fact]
    public async Task SR11_CompleteRequest_ProviderRole_Returns200()
    {
        SetProviderToken(ProviderUserId);
        var requestId = Guid.NewGuid();

        var response = await _client.PutAsync(
            $"{BaseRoute}/{requestId}/complete",
            new StringContent(string.Empty, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _requestServiceMock.Verify(s => s.CompleteRequestAsync(requestId, TestProviderId), Times.Once);
    }

    // ════════════════════════════════════════════════════════════════════════
    // SHARED + ADMIN ENDPOINTS
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SR12_GetRequestDetails_NoToken_Returns200()
    {
        ClearToken();
        var requestId = Guid.NewGuid();

        var response = await _client.GetAsync($"{BaseRoute}/{requestId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(requestId, body.GetProperty("data").GetProperty("id").GetGuid());

        _requestServiceMock.Verify(s => s.GetRequestDetailsAsync(requestId), Times.Once);
    }

    [Fact]
    public async Task SR13_GetAllPlatformRequests_AdminRole_Returns200()
    {
        SetAdminToken("service-request-admin-user");

        var response = await _client.GetAsync($"{BaseRoute}/admin?pageIndex=1&pageSize=4");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(4, body.GetProperty("data").GetProperty("pageSize").GetInt32());

        _requestServiceMock.Verify(
            s => s.GetAllPlatformRequestsAsync(It.Is<RequestQueryParams>(q => q.PageIndex == 1 && q.PageSize == 4)),
            Times.Once);
    }

    [Fact]
    public async Task SR14_GetAllPlatformRequests_ProviderRole_Returns403()
    {
        SetProviderToken();

        var response = await _client.GetAsync($"{BaseRoute}/admin");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
