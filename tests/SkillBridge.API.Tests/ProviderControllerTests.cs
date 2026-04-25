using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
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
using SoftBridge.Domain.Exceptions.NotFoundModels;
using SoftBridge.Shared.Common.Dto.ServiceProvider;
using SoftBridge.Web;
using System.Security.Claims;
using Xunit;

namespace SoftBridge.API.Tests.Controllers;

/// <summary>
/// Integration tests for ProviderController.
/// Routes:
///   GET    api/provider/me
///   PUT    api/provider/me
///   DELETE api/provider/me
/// </summary>
public class ProviderControllerTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly Mock<IProviderProfileService> _providerServiceMock;
    private readonly Mock<IRequestWorkflowService> _requestWorkflowServiceMock;

    // JWT settings must match appsettings.json
    private const string JwtKey = "7zzJSeUi5eL3PfoQt7XieaSsbzIZ3CS5juJNbW4kouJx6Bpdkt";
    private const string JwtIssuer = "http://localhost:5000";
    private const string JwtAudience = "SoftBridge";

    private const string BaseRoute = "api/provider";

    private static readonly string DbName = "ProviderTestDb_" + Guid.NewGuid();

    public ProviderControllerTests(WebApplicationFactory<Program> factory)
    {
        _providerServiceMock = new Mock<IProviderProfileService>();
        _requestWorkflowServiceMock = new Mock<IRequestWorkflowService>();

        _providerServiceMock
            .Setup(s => s.GetMyProfileAsync(It.IsAny<string>()))
            .ReturnsAsync((string userId) => new ProviderProfileDto
            {
                Id = Guid.NewGuid(),
                FullName = "Provider Test",
                Email = $"{userId}@example.com",
                Bio = "Test bio",
                PortfolioLink = "https://portfolio.example.com",
                ProfileImageUrl = "https://cdn.example.com/profile.png",
                CvUrl = "https://cdn.example.com/cv.pdf",
                AccountStatus = "Active",
                AverageRating = 4.5f,
                TotalReviews = 10,
                ApprovedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            });

        _providerServiceMock
            .Setup(s => s.UpdateProfileAsync(It.IsAny<string>(), It.IsAny<UpdateProviderProfileDto>()))
            .ReturnsAsync((string userId, UpdateProviderProfileDto dto) => new ProviderProfileDto
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = $"{userId}@example.com",
                Bio = dto.Bio,
                PortfolioLink = dto.PortfolioLink,
                ProfileImageUrl = "https://cdn.example.com/profile.png",
                CvUrl = "https://cdn.example.com/cv.pdf",
                AccountStatus = "Active",
                AverageRating = 4.5f,
                TotalReviews = 10,
                ApprovedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            });

        _providerServiceMock
            .Setup(s => s.DeleteAccountAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

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
                services.RemoveAll<IProviderProfileService>();
                services.AddSingleton(_providerServiceMock.Object);

                services.RemoveAll<IRequestWorkflowService>();
                services.AddSingleton(_requestWorkflowServiceMock.Object);
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
    // GET api/provider/me
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task PR01_GetMyProfile_ProviderRole_Returns200()
    {
        const string userId = "provider-user-01";
        SetProviderToken(userId);

        var response = await _client.GetAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
        Assert.Equal("Provider Test", body.GetProperty("data").GetProperty("fullName").GetString());

        _providerServiceMock.Verify(s => s.GetMyProfileAsync(userId), Times.Once);
    }

    [Fact]
    public async Task PR02_GetMyProfile_NoToken_Returns401()
    {
        ClearToken();
        var response = await _client.GetAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PR03_GetMyProfile_ClientRole_Returns403()
    {
        SetClientToken();
        var response = await _client.GetAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PR04_GetMyProfile_AdminRole_Returns403()
    {
        SetAdminToken();
        var response = await _client.GetAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PR05_GetMyProfile_NotFound_Returns404()
    {
        const string userId = "provider-not-found";

        _providerServiceMock
            .Setup(s => s.GetMyProfileAsync(userId))
            .ThrowsAsync(new ProviderNotFoundException("Provider profile was not found."));

        SetProviderToken(userId);
        var response = await _client.GetAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.False(body.GetProperty("isSuccess").GetBoolean());
    }

    // ════════════════════════════════════════════════════════════════════════
    // PUT api/provider/me
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task PR06_UpdateProfile_ProviderRole_Returns200()
    {
        const string userId = "provider-update-01";
        SetProviderToken(userId);

        using var content = ValidUpdateContent();
        var response = await _client.PutAsync($"{BaseRoute}/me", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
        Assert.Equal("Updated Provider", body.GetProperty("data").GetProperty("fullName").GetString());

        _providerServiceMock.Verify(
            s => s.UpdateProfileAsync(
                userId,
                It.Is<UpdateProviderProfileDto>(d =>
                    d.FullName == "Updated Provider" &&
                    d.Bio == "Experienced provider" &&
                    d.PortfolioLink == "https://portfolio.example.com")),
            Times.Once);
    }

    [Fact]
    public async Task PR07_UpdateProfile_NoToken_Returns401()
    {
        ClearToken();
        using var content = ValidUpdateContent();

        var response = await _client.PutAsync($"{BaseRoute}/me", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PR08_UpdateProfile_ClientRole_Returns403()
    {
        SetClientToken();
        using var content = ValidUpdateContent();

        var response = await _client.PutAsync($"{BaseRoute}/me", content);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // DELETE api/provider/me
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task PR09_DeleteAccount_ProviderRole_Returns200()
    {
        const string userId = "provider-delete-01";
        SetProviderToken(userId);

        var response = await _client.DeleteAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());

        _providerServiceMock.Verify(s => s.DeleteAccountAsync(userId), Times.Once);
    }

    [Fact]
    public async Task PR10_DeleteAccount_NoToken_Returns401()
    {
        ClearToken();
        var response = await _client.DeleteAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PR11_DeleteAccount_ClientRole_Returns403()
    {
        SetClientToken();
        var response = await _client.DeleteAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
