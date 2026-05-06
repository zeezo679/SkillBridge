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
using SoftBridge.Abstraction.IServicesContract.Review;
using SoftBridge.Shared.Common.Dto.Client;
using SoftBridge.Web;
using System.Security.Claims;
using Xunit;

namespace SoftBridge.API.Tests.Controllers;

/// <summary>
/// Mock-based integration tests for ClientController.
/// Routes:
///   GET    api/client/me
///   PUT    api/client/me
///   DELETE api/client/me
/// </summary>
public class ClientControllerTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly Mock<IClientProfileService> _clientServiceMock;
    private readonly Mock<IRequestWorkflowService> _requestWorkflowServiceMock;
    private readonly Mock<IReviewService> _reviewServiceMock;

    private const string JwtKey = "7zzJSeUi5eL3PfoQt7XieaSsbzIZ3CS5juJNbW4kouJx6Bpdkt";
    private const string JwtIssuer = "http://localhost:5000";
    private const string JwtAudience = "SoftBridge";

    private const string BaseRoute = "api/client";

    private static readonly string DbName = "ClientTestDb_" + Guid.NewGuid();

    public ClientControllerTests(WebApplicationFactory<Program> factory)
    {
        _clientServiceMock = new Mock<IClientProfileService>();
        _requestWorkflowServiceMock = new Mock<IRequestWorkflowService>();
        _reviewServiceMock = new Mock<IReviewService>();

        _clientServiceMock
            .Setup(s => s.GetMyProfileAsync(It.IsAny<string>()))
            .ReturnsAsync((string userId) => new ClientProfileDto
            {
                Id = Guid.NewGuid(),
                Name = "Client Test",
                Email = $"{userId}@example.com",
                ProfileImageUrl = "https://cdn.example.com/client.png",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            });

        _clientServiceMock
            .Setup(s => s.UpdateProfileAsync(It.IsAny<string>(), It.IsAny<UpdateClientProfileDto>()))
            .ReturnsAsync((string userId, UpdateClientProfileDto dto) => new ClientProfileDto
            {
                Id = Guid.NewGuid(),
                Name = dto.FullName,
                Email = $"{userId}@example.com",
                ProfileImageUrl = "https://cdn.example.com/client-updated.png",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            });

        _clientServiceMock
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
                services.RemoveAll<IClientProfileService>();
                services.AddSingleton(_clientServiceMock.Object);

                services.RemoveAll<IRequestWorkflowService>();
                services.AddSingleton(_requestWorkflowServiceMock.Object);

                services.RemoveAll<IReviewService>();
                services.AddSingleton(_reviewServiceMock.Object);
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
    private void SetAdminToken(string? userId = null) => SetToken("Admin", userId);
    private void ClearToken() => _client.DefaultRequestHeaders.Authorization = null;

    private void SetToken(string role, string? userId = null) =>
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateJwt(role, userId));

    private static MultipartFormDataContent ValidUpdateContent(string fullName = "Updated Client")
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(fullName), "FullName");
        return content;
    }

    [Fact]
    public async Task CL01_GetMyProfile_ClientRole_Returns200()
    {
        const string userId = "client-user-01";
        SetClientToken(userId);

        var response = await _client.GetAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
        Assert.Equal("Client Test", body.GetProperty("data").GetProperty("name").GetString());

        _clientServiceMock.Verify(s => s.GetMyProfileAsync(userId), Times.Once);
    }

    [Fact]
    public async Task CL02_GetMyProfile_NoToken_Returns401()
    {
        ClearToken();

        var response = await _client.GetAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CL03_GetMyProfile_AdminRole_Returns403()
    {
        SetAdminToken("client-admin-01");

        var response = await _client.GetAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CL04_UpdateProfile_ClientRole_Returns200()
    {
        const string userId = "client-update-01";
        SetClientToken(userId);
        using var content = ValidUpdateContent();

        var response = await _client.PutAsync($"{BaseRoute}/me", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
        Assert.Equal("Updated Client", body.GetProperty("data").GetProperty("name").GetString());

        _clientServiceMock.Verify(
            s => s.UpdateProfileAsync(userId, It.Is<UpdateClientProfileDto>(d => d.FullName == "Updated Client")),
            Times.Once);
    }

    [Fact]
    public async Task CL05_UpdateProfile_NoToken_Returns401()
    {
        ClearToken();
        using var content = ValidUpdateContent();

        var response = await _client.PutAsync($"{BaseRoute}/me", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CL06_DeleteAccount_ClientRole_Returns200()
    {
        const string userId = "client-delete-01";
        SetClientToken(userId);

        var response = await _client.DeleteAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());

        _clientServiceMock.Verify(s => s.DeleteAccountAsync(userId), Times.Once);
    }

    [Fact]
    public async Task CL07_DeleteAccount_NoToken_Returns401()
    {
        ClearToken();

        var response = await _client.DeleteAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
