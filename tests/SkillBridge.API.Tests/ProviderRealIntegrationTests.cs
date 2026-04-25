using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SoftBridge.Web;
using Xunit;

namespace SoftBridge.API.Tests.Controllers;

/// <summary>
/// Real-services integration tests for ProviderController.
/// Uses the full DI graph with InMemory database and no service mocks.
/// </summary>
public class ProviderRealIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;

    // JWT settings must match appsettings.json
    private const string JwtKey = "7zzJSeUi5eL3PfoQt7XieaSsbzIZ3CS5juJNbW4kouJx6Bpdkt";
    private const string JwtIssuer = "http://localhost:5000";
    private const string JwtAudience = "SoftBridge";

    private const string BaseRoute = "api/provider";

    private static readonly string DbName = "ProviderRealTestDb_" + Guid.NewGuid();

    public ProviderRealIntegrationTests(WebApplicationFactory<Program> factory)
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

    private void SetProviderToken(string? userId = null) => SetToken("Provider", userId);
    private void SetClientToken(string? userId = null) => SetToken("Client", userId);
    private void SetAdminToken(string? userId = null) => SetToken("Admin", userId);
    private void ClearToken() => _client.DefaultRequestHeaders.Authorization = null;

    private void SetToken(string role, string? userId = null) =>
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateJwt(role, userId));

    private static MultipartFormDataContent ValidUpdateContent(
        string fullName = "Updated Provider",
        string bio = "Experienced provider",
        string portfolio = "https://portfolio.example.com")
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(fullName), "FullName");
        content.Add(new StringContent(bio), "Bio");
        content.Add(new StringContent(portfolio), "PortfolioLink");
        return content;
    }

    // ════════════════════════════════════════════════════════════════════════
    // REAL SERVICE + INMEMORY SCENARIOS
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task PRR01_GetMyProfile_NoToken_Returns401()
    {
        ClearToken();

        var response = await _client.GetAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PRR02_GetMyProfile_ClientRole_Returns403()
    {
        SetClientToken("provider-real-client-role");

        var response = await _client.GetAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PRR03_GetMyProfile_AdminRole_Returns403()
    {
        SetAdminToken("provider-real-admin-role");

        var response = await _client.GetAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PRR04_GetMyProfile_ProviderWithoutProfile_Returns404()
    {
        SetProviderToken("provider-real-missing-profile");

        var response = await _client.GetAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.False(body.GetProperty("isSuccess").GetBoolean());
    }

    [Fact]
    public async Task PRR05_UpdateProfile_ProviderWithoutProfile_Returns404()
    {
        SetProviderToken("provider-real-update-missing");
        using var content = ValidUpdateContent();

        var response = await _client.PutAsync($"{BaseRoute}/me", content);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.False(body.GetProperty("isSuccess").GetBoolean());
    }

    [Fact]
    public async Task PRR06_DeleteAccount_ProviderWithoutProfile_Returns404()
    {
        SetProviderToken("provider-real-delete-missing");

        var response = await _client.DeleteAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.False(body.GetProperty("isSuccess").GetBoolean());
    }
}
