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
using Microsoft.IdentityModel.Tokens;
using SoftBridge.Shared.Common.Dto.ServiceRequest.NewDtos;
using SoftBridge.Web;
using Xunit;

namespace SoftBridge.API.Tests.Controllers;

/// <summary>
/// Real-services integration tests for RequestController.
/// Uses the full DI graph with InMemory database and no service mocks.
/// </summary>
public class ServiceRequestRealIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;

    // JWT settings must match appsettings.json
    private const string JwtKey = "7zzJSeUi5eL3PfoQt7XieaSsbzIZ3CS5juJNbW4kouJx6Bpdkt";
    private const string JwtIssuer = "http://localhost:5000";
    private const string JwtAudience = "SoftBridge";

    private const string BaseRoute = "api/request";

    private static readonly string DbName = "ServiceRequestRealTestDb_" + Guid.NewGuid();

    public ServiceRequestRealIntegrationTests(WebApplicationFactory<Program> factory)
    {
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
        }).CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    // ════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════════════════════

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

    private void SetAdminToken(string? userId = null) => SetToken("Admin", userId);
    private void SetClientToken(string? userId = null) => SetToken("Client", userId);
    private void SetProviderToken(string? userId = null) => SetToken("Provider", userId);
    private void ClearToken() => _client.DefaultRequestHeaders.Authorization = null;

    private void SetToken(string role, string? userId = null) =>
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateJwt(role, userId));

    // ════════════════════════════════════════════════════════════════════════
    // REAL SERVICE + INMEMORY SCENARIOS
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task RSR01_GetRequestDetails_NonExistentRequest_Returns404()
    {
        ClearToken();

        var response = await _client.GetAsync($"{BaseRoute}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.False(body.GetProperty("isSuccess").GetBoolean());
    }

    [Fact]
    public async Task RSR02_GetAllPlatformRequests_AdminRole_EmptyDb_Returns200()
    {
        SetAdminToken("real-admin-01");

        var response = await _client.GetAsync($"{BaseRoute}/admin?pageIndex=1&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(1, body.GetProperty("data").GetProperty("pageIndex").GetInt32());
        Assert.Equal(5, body.GetProperty("data").GetProperty("pageSize").GetInt32());
    }

    [Fact]
    public async Task RSR03_SendRequest_ClientWithoutProfile_Returns404()
    {
        SetClientToken("real-client-without-profile");

        var payload = new CreateRequestDto
        {
            ServiceId = Guid.NewGuid(),
            Message = "Need service",
            AgreedPrice = 100
        };

        var response = await _client.PostAsJsonAsync(BaseRoute, payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.False(body.GetProperty("isSuccess").GetBoolean());
    }

    [Fact]
    public async Task RSR04_GetProviderRequests_ProviderWithoutProfile_Returns404()
    {
        SetProviderToken("real-provider-without-profile");

        var response = await _client.GetAsync($"{BaseRoute}/Provider?pageIndex=1&pageSize=3");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.False(body.GetProperty("isSuccess").GetBoolean());
    }

    [Fact]
    public async Task RSR05_GetAllPlatformRequests_ProviderRole_Returns403()
    {
        SetProviderToken("real-provider-01");

        var response = await _client.GetAsync($"{BaseRoute}/admin");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
