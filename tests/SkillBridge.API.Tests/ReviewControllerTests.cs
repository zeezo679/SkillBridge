using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using E_commerce.Shared.Common.Dto.Review;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using SoftBridge.Abstraction.IServices.Profiles;
using SoftBridge.Abstraction.IServicesContract.Review;
using SoftBridge.Shared.Common.Dto.Client;
using SoftBridge.Shared.Common.Dto.Review;
using SoftBridge.Web;
using Xunit;

namespace SoftBridge.API.Tests.Controllers;

/// <summary>
/// Mock-based integration tests for ReviewController.
/// Routes:
///   GET    api/review/{id}
///   GET    api/review/service/{serviceId}
///   GET    api/review/provider/{providerId}
///   GET    api/review/me
///   POST   api/review
///   PUT    api/review/{id}
///   DELETE api/review/{id}
/// </summary>
public class ReviewControllerTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly Mock<IReviewService> _reviewServiceMock;
    private readonly Mock<IClientProfileService> _clientServiceMock;

    private const string JwtKey = "7zzJSeUi5eL3PfoQt7XieaSsbzIZ3CS5juJNbW4kouJx6Bpdkt";
    private const string JwtIssuer = "http://localhost:5000";
    private const string JwtAudience = "SoftBridge";

    private const string BaseRoute = "api/review";

    private static readonly string DbName = "ReviewTestDb_" + Guid.NewGuid();

    public ReviewControllerTests(WebApplicationFactory<Program> factory)
    {
        _reviewServiceMock = new Mock<IReviewService>();
        _clientServiceMock = new Mock<IClientProfileService>();

        _clientServiceMock
            .Setup(s => s.GetMyProfileAsync(It.IsAny<string>()))
            .ReturnsAsync((string userId) => new ClientProfileDto
            {
                Id = Guid.NewGuid(),
                Name = "Review Client",
                Email = $"{userId}@example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            });

        _reviewServiceMock
            .Setup(s => s.GetReviewByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid reviewId) => BuildReviewDto(reviewId: reviewId));

        _reviewServiceMock
            .Setup(s => s.GetReviewsByServiceAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid serviceId) => new List<ReviewDto>
            {
                BuildReviewDto(serviceId: serviceId),
                BuildReviewDto(serviceId: serviceId)
            });

        _reviewServiceMock
            .Setup(s => s.GetReviewsByProviderAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid providerId) => new List<ReviewDto>
            {
                BuildReviewDto(providerId: providerId)
            });

        _reviewServiceMock
            .Setup(s => s.GetMyReviewsAsync(It.IsAny<string>()))
            .ReturnsAsync((string _) => new List<ReviewDto>
            {
                BuildReviewDto(),
                BuildReviewDto()
            });

        _reviewServiceMock
            .Setup(s => s.AddReviewAsync(It.IsAny<string>(), It.IsAny<AddReviewDto>()))
            .ReturnsAsync((string _, AddReviewDto dto) => BuildReviewDto(requestId: dto.RequestId, rating: dto.Rating, comment: dto.Comment));

        _reviewServiceMock
            .Setup(s => s.UpdateReviewAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<UpdateReviewDto>()))
            .ReturnsAsync((string _, Guid reviewId, UpdateReviewDto dto) => BuildReviewDto(reviewId: reviewId, rating: dto.Rating, comment: dto.Comment));

        _reviewServiceMock
            .Setup(s => s.DeleteReviewAsync(It.IsAny<string>(), It.IsAny<Guid>()))
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
                services.RemoveAll<IReviewService>();
                services.AddSingleton(_reviewServiceMock.Object);

                services.RemoveAll<IClientProfileService>();
                services.AddSingleton(_clientServiceMock.Object);
            });
        }).CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    private static ReviewDto BuildReviewDto(
        Guid? reviewId = null,
        Guid? requestId = null,
        Guid? serviceId = null,
        Guid? providerId = null,
        byte rating = 5,
        string? comment = "Excellent")
    {
        return new ReviewDto
        {
            Id = reviewId ?? Guid.NewGuid(),
            RequestId = requestId ?? Guid.NewGuid(),
            ServiceId = serviceId ?? Guid.NewGuid(),
            ServiceTitle = "Service Review Test",
            ProviderId = providerId ?? Guid.NewGuid(),
            ProviderName = "Provider Test",
            ClientId = Guid.NewGuid(),
            ClientName = "Client Test",
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };
    }

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
    public async Task RV01_GetById_AllowAnonymous_Returns200()
    {
        var id = Guid.NewGuid();

        var response = await _client.GetAsync($"{BaseRoute}/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _reviewServiceMock.Verify(s => s.GetReviewByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task RV02_GetByService_AllowAnonymous_Returns200()
    {
        var serviceId = Guid.NewGuid();

        var response = await _client.GetAsync($"{BaseRoute}/service/{serviceId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(2, body.GetProperty("data").GetArrayLength());

        _reviewServiceMock.Verify(s => s.GetReviewsByServiceAsync(serviceId), Times.Once);
    }

    [Fact]
    public async Task RV03_GetByProvider_AllowAnonymous_Returns200()
    {
        var providerId = Guid.NewGuid();

        var response = await _client.GetAsync($"{BaseRoute}/provider/{providerId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _reviewServiceMock.Verify(s => s.GetReviewsByProviderAsync(providerId), Times.Once);
    }

    [Fact]
    public async Task RV04_GetMyReviews_ClientRole_Returns200()
    {
        const string userId = "review-client-01";
        SetClientToken(userId);

        var response = await _client.GetAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _reviewServiceMock.Verify(s => s.GetMyReviewsAsync(userId), Times.Once);
    }

    [Fact]
    public async Task RV05_GetMyReviews_NoToken_Returns401()
    {
        ClearToken();

        var response = await _client.GetAsync($"{BaseRoute}/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RV06_AddReview_ClientRole_Returns200()
    {
        const string userId = "review-add-client";
        SetClientToken(userId);

        var payload = new AddReviewDto
        {
            RequestId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Great service"
        };

        var response = await _client.PostAsJsonAsync(BaseRoute, payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _reviewServiceMock.Verify(
            s => s.AddReviewAsync(userId, It.Is<AddReviewDto>(d => d.RequestId == payload.RequestId && d.Rating == 5)),
            Times.Once);
    }

    [Fact]
    public async Task RV07_AddReview_ProviderRole_Returns403()
    {
        SetProviderToken("review-provider-role");

        var payload = new AddReviewDto
        {
            RequestId = Guid.NewGuid(),
            Rating = 4,
            Comment = "Not allowed"
        };

        var response = await _client.PostAsJsonAsync(BaseRoute, payload);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RV08_UpdateReview_ClientRole_Returns200()
    {
        const string userId = "review-update-client";
        SetClientToken(userId);

        var reviewId = Guid.NewGuid();
        var payload = new { rating = 4, comment = "Updated comment" };

        var response = await _client.PutAsJsonAsync($"{BaseRoute}/{reviewId}", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _reviewServiceMock.Verify(
            s => s.UpdateReviewAsync(userId, reviewId, It.Is<UpdateReviewDto>(d => d.Rating == 4 && d.Comment == "Updated comment")),
            Times.Once);
    }

    [Fact]
    public async Task RV09_DeleteReview_ClientRole_Returns200()
    {
        const string userId = "review-delete-client";
        SetClientToken(userId);

        var reviewId = Guid.NewGuid();
        var response = await _client.DeleteAsync($"{BaseRoute}/{reviewId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        _reviewServiceMock.Verify(s => s.DeleteReviewAsync(userId, reviewId), Times.Once);
    }

    [Fact]
    public async Task RV10_DeleteReview_NoToken_Returns401()
    {
        ClearToken();

        var response = await _client.DeleteAsync($"{BaseRoute}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
