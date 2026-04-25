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
using SoftBridge.Shared.Common.Dto.Review;
using SoftBridge.Web;
using Xunit;

namespace SoftBridge.API.Tests.Controllers;

/// <summary>
/// Real-services integration tests for ReviewController.
/// Uses full DI graph with InMemory database and no service mocks.
/// </summary>
public class ReviewRealIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;

    private const string JwtKey = "7zzJSeUi5eL3PfoQt7XieaSsbzIZ3CS5juJNbW4kouJx6Bpdkt";
    private const string JwtIssuer = "http://localhost:5000";
    private const string JwtAudience = "SoftBridge";

    private const string BaseRoute = "api/review";

    private static readonly string DbName = "ReviewRealTestDb_" + Guid.NewGuid();

    public ReviewRealIntegrationTests(WebApplicationFactory<Program> factory)
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

    private void SetClientToken(string? userId = null) => SetToken("Client", userId);
    private void SetProviderToken(string? userId = null) => SetToken("Provider", userId);
    private void ClearToken() => _client.DefaultRequestHeaders.Authorization = null;

    private void SetToken(string role, string? userId = null) =>
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateJwt(role, userId));

    [Fact]
    public async Task RVR01_GetById_NonExistent_Returns404()
    {
        var response = await _client.GetAsync($"{BaseRoute}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.False(body.GetProperty("isSuccess").GetBoolean());
    }

    [Fact]
    public async Task RVR02_GetByService_EmptyDb_Returns200()
    {
        var response = await _client.GetAsync($"{BaseRoute}/service/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(0, body.GetProperty("data").GetArrayLength());
    }

    [Fact]
    public async Task RVR03_GetByProvider_EmptyDb_Returns200()
    {
        var response = await _client.GetAsync($"{BaseRoute}/provider/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
    }

    [Fact]
    public async Task RVR04_GetMyReviews_NoToken_Returns401()
    {
        ClearToken();

        var response = await _client.GetAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RVR05_GetMyReviews_ClientWithoutProfile_Returns404()
    {
        SetClientToken("review-real-missing-client");

        var response = await _client.GetAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RVR06_AddReview_NoToken_Returns401()
    {
        ClearToken();

        var payload = new AddReviewDto { RequestId = Guid.NewGuid(), Rating = 5, Comment = "great" };
        var response = await _client.PostAsJsonAsync(BaseRoute, payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RVR07_AddReview_ProviderRole_Returns403()
    {
        SetProviderToken("review-real-provider-role");

        var payload = new AddReviewDto { RequestId = Guid.NewGuid(), Rating = 5, Comment = "great" };
        var response = await _client.PostAsJsonAsync(BaseRoute, payload);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RVR08_AddReview_ClientWithoutProfile_Returns404()
    {
        SetClientToken("review-real-add-missing");

        var payload = new AddReviewDto { RequestId = Guid.NewGuid(), Rating = 5, Comment = "great" };
        var response = await _client.PostAsJsonAsync(BaseRoute, payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RVR09_UpdateReview_ClientWithoutProfile_Returns404()
    {
        SetClientToken("review-real-update-missing");

        var response = await _client.PutAsJsonAsync(
            $"{BaseRoute}/{Guid.NewGuid()}",
            new { rating = 4, comment = "updated" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RVR10_DeleteReview_ClientWithoutProfile_Returns404()
    {
        SetClientToken("review-real-delete-missing");

        var response = await _client.DeleteAsync($"{BaseRoute}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
