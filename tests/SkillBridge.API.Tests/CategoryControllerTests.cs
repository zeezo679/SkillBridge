using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;
using SoftBridge.Web;
using SoftBridge.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

// ============================================================
// Adjust the namespace to match your test project name
// ============================================================
namespace SoftBridge.API.Tests.Controllers;

/// <summary>
/// Integration tests for CategoryController.
/// Uses WebApplicationFactory to spin up the real pipeline in-memory.
/// SQL Server is replaced with an EF Core InMemory database for isolation.
/// </summary>
public class CategoryControllerTests : IClassFixture<WebApplicationFactory<SoftBridge.Web.Program>>,
    IAsyncLifetime
{
    private readonly HttpClient _client;

    // ── JWT settings must match appsettings.json exactly ──────────────────
    private const string JwtKey      = "7zzJSeUi5eL3PfoQt7XieaSsbzIZ3CS5juJNbW4kouJx6Bpdkt";
    private const string JwtIssuer   = "http://localhost:5000";
    private const string JwtAudience = "SoftBridge";

    // ── Base route derived from [Route("api/[controller]")] ───────────────
    // Controller class name = CategoryController → route = api/category
    private const string BaseRoute = "api/category";

    public CategoryControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
{
    builder.UseEnvironment("Development"); // Ensure we use development settings (e.g., for JWT validation)

    builder.ConfigureAppConfiguration((_, config) =>
    {
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JwtTokenSettings:Key"]      = "7zzJSeUi5eL3PfoQt7XieaSsbzIZ3CS5juJNbW4kouJx6Bpdkt",
            ["JwtTokenSettings:Issuer"]   = "http://localhost:5000",
            ["JwtTokenSettings:Audience"] = "SoftBridge",
        });
    });

    builder.ConfigureServices(services =>
    {
        // ✅ Remove the ENTIRE DbContext registration, not just the options
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<ProjectDbContext>));
        if (descriptor != null)
            services.Remove(descriptor);

        // Also remove the DbContext itself
        var contextDescriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(ProjectDbContext));
        if (contextDescriptor != null)
            services.Remove(contextDescriptor);

        // ✅ Now add a fresh one with InMemory only
        services.AddDbContext<ProjectDbContext>(opt =>
            opt.UseInMemoryDatabase("CategoryTestDb_" + Guid.NewGuid()));
        });
            }).CreateClient();
    }

    // IAsyncLifetime: runs before each test class (nothing to seed here)
    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    // ════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Generates a signed JWT with the given role claim.</summary>
    private static string GenerateJwt(string role)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer:             JwtIssuer,
            audience:           JwtAudience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private void SetAdminToken()    => SetToken("Admin");
    private void SetClientToken()   => SetToken("Client");
    private void SetProviderToken() => SetToken("Provider");
    private void ClearToken()       => _client.DefaultRequestHeaders.Authorization = null;

    private void SetToken(string role) =>
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateJwt(role));

    /// <summary>
    /// Builds a valid CategoryToCreateDto payload.
    /// IconUrl must be a valid URL because of [Url] annotation on the DTO.
    /// </summary>
    private static object ValidCreatePayload(string name = "Web Development") => new
    {
        Name    = name,
        IconUrl = "https://cdn.example.com/icons/web.png"
    };

    /// <summary>
    /// Builds a valid CategoryToUpdateDto payload.
    /// IsActive defaults to true on the DTO.
    /// </summary>
    private static object ValidUpdatePayload(string name = "Updated Name", bool isActive = true) => new
    {
        Name     = name,
        IconUrl  = "https://cdn.example.com/icons/updated.png",
        IsActive = isActive
    };

    private static StringContent ToJson(object obj) =>
        new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    /// <summary>
    /// Creates a category as Admin and returns its GUID string from the response body.
    /// Useful for tests that need a pre-existing category.
    /// </summary>
    // In CreateCategoryAndGetId helper, replace EnsureSuccessStatusCode() with this:
private async Task<string> CreateCategoryAndGetId(string name = "Seed Category")
{
    SetAdminToken();
    var response = await _client.PostAsync(BaseRoute, ToJson(ValidCreatePayload(name)));
    
    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadAsStringAsync();
        throw new Exception($"CreateCategory failed [{response.StatusCode}]: {error}");
    }

    var body = await response.Content.ReadAsStringAsync();
    return JsonDocument.Parse(body)
        .RootElement
        .GetProperty("data")
        .GetProperty("id")
        .GetString()!;
}

    // ════════════════════════════════════════════════════════════════════════
    // A01 — Create: valid payload → 201
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A01_Create_ValidPayload_Returns201()
    {
        SetAdminToken();

        var response = await _client.PostAsync(BaseRoute, ToJson(ValidCreatePayload()));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
        Assert.True(Guid.TryParse(body.GetProperty("data").GetProperty("id").GetString(), out _),
            "Id must be a valid GUID");
    }


   

    // ════════════════════════════════════════════════════════════════════════
    // A03 — Create: empty Name → 400 (DataAnnotations: [Required])
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A03_Create_EmptyName_Returns400()
    {
        SetAdminToken();

        var payload = new { Name = "", IconUrl = "https://cdn.example.com/icon.png" };
        var response = await _client.PostAsync(BaseRoute, ToJson(payload));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A04 — Create: Name > 100 chars → 400 (DataAnnotations: [MaxLength(100)])
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A04_Create_NameTooLong_Returns400()
    {
        SetAdminToken();

        var payload = new { Name = new string('A', 101), IconUrl = "https://cdn.example.com/icon.png" };
        var response = await _client.PostAsync(BaseRoute, ToJson(payload));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A04b — Create: invalid IconUrl (not a URL) → 400 ([Url] annotation)
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A04b_Create_InvalidIconUrl_Returns400()
    {
        SetAdminToken();

        var payload = new { Name = "Valid Name", IconUrl = "not-a-url" };
        var response = await _client.PostAsync(BaseRoute, ToJson(payload));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A05 — GetAll: paginated response → 200
    // Route: GET api/category  (no sub-path, matches [HttpGet])
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A05_GetAll_Paginated_Returns200()
    {
        SetAdminToken();
        await CreateCategoryAndGetId("Category One");
        await CreateCategoryAndGetId("Category Two");

        ClearToken(); // GetAll is public
        var response = await _client.GetAsync($"{BaseRoute}?pageIndex=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());

        // PaginationResponse<CategoryDto> must have a data property with items
        var data = body.GetProperty("data");
        Assert.True(data.TryGetProperty("data", out _), "Response must contain paginated data array");
    }

    // ════════════════════════════════════════════════════════════════════════
    // A05b — GetAll: name filter works
    // CategoryQueryParams has a Name property used in CategoryFiltrationSpecification
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A05b_GetAll_FilterByName_ReturnsMatch()
    {
        SetAdminToken();
        await CreateCategoryAndGetId("Mobile Development");
        await CreateCategoryAndGetId("Machine Learning");

        ClearToken();
        var response = await _client.GetAsync($"{BaseRoute}?name=Mobile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Mobile Development", body, StringComparison.OrdinalIgnoreCase);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A06 — GetById: valid Id → 200
    // Route: GET api/category/categories/{id}  (matches [HttpGet("categories/{id}")])
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A06_GetById_ValidId_Returns200()
    {
        var id = await CreateCategoryAndGetId("Backend Dev");

        ClearToken();
        var response = await _client.GetAsync($"{BaseRoute}/categories/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(id, body.GetProperty("data").GetProperty("id").GetString());
    }

    // ════════════════════════════════════════════════════════════════════════
    // A07 — GetById: non-existent Id → 404
    // CategoryService throws CategoryNotFoundException → GlobalErrorHandler → 404
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A07_GetById_NonExistentId_Returns404()
    {
        ClearToken();
        var response = await _client.GetAsync($"{BaseRoute}/categories/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A08 — Update: valid payload → 200
    // Route: PUT api/category/{id}
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A08_Update_ValidPayload_Returns200()
    {
        var id = await CreateCategoryAndGetId("Old Name");

        SetAdminToken();
        var response = await _client.PutAsync($"{BaseRoute}/{id}", ToJson(ValidUpdatePayload("New Name")));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify change persisted
        ClearToken();
        var getResponse = await _client.GetAsync($"{BaseRoute}/categories/{id}");
        var body        = await getResponse.Content.ReadAsStringAsync();
        Assert.Contains("New Name", body);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A09 — Update: non-existent Id → 404
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A09_Update_NonExistentId_Returns404()
    {
        SetAdminToken();
        var response = await _client.PutAsync(
            $"{BaseRoute}/{Guid.NewGuid()}",
            ToJson(ValidUpdatePayload()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A10 — Update: IsActive=false soft-deactivates category
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A10_Update_SetIsActiveFalse_DeactivatesCategory()
    {
        var id = await CreateCategoryAndGetId("To Deactivate");

        SetAdminToken();
        await _client.PutAsync($"{BaseRoute}/{id}", ToJson(ValidUpdatePayload("To Deactivate", isActive: false)));

        // Verify IsActive=false is persisted (CategoryFiltrationSpecification filters by IsActive)
        ClearToken();
        var listing = await _client.GetAsync($"{BaseRoute}?isActive=true");
        var body    = await listing.Content.ReadAsStringAsync();
        Assert.DoesNotContain("To Deactivate", body);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A11 — Delete: valid Id with no services → 200
    // Route: DELETE api/category/{id}
    // CategoryService hard-deletes (no soft delete in current implementation)
    // and throws BadRequestExceptionCustome if category has services
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A11_Delete_ValidIdNoServices_Returns200()
    {
        var id = await CreateCategoryAndGetId("To Delete");

        SetAdminToken();
        var response = await _client.DeleteAsync($"{BaseRoute}/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A11b — Delete: non-existent Id → 404
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A11b_Delete_NonExistentId_Returns404()
    {
        SetAdminToken();
        var response = await _client.DeleteAsync($"{BaseRoute}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A11c — Delete: deleted category no longer appears in listing
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A11c_Delete_CategoryRemovedFromListing()
    {
        var id = await CreateCategoryAndGetId("Hidden After Delete");

        SetAdminToken();
        await _client.DeleteAsync($"{BaseRoute}/{id}");

        ClearToken();
        var listing = await _client.GetAsync(BaseRoute);
        var body    = await listing.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Hidden After Delete", body);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A12 — Auth: no token on POST → 401
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A12_Create_NoToken_Returns401()
    {
        ClearToken();
        var response = await _client.PostAsync(BaseRoute, ToJson(ValidCreatePayload()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A13 — RBAC: Client role on POST → 403
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A13_Create_ClientRole_Returns403()
    {
        SetClientToken();
        var response = await _client.PostAsync(BaseRoute, ToJson(ValidCreatePayload()));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A14 — RBAC: Provider role on POST → 403
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A14_Create_ProviderRole_Returns403()
    {
        SetProviderToken();
        var response = await _client.PostAsync(BaseRoute, ToJson(ValidCreatePayload()));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A15 — RBAC: Admin token succeeds on all write operations
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A15_AdminToken_AllCrudSucceeds()
    {
        SetAdminToken();

        // Create
        var createResp = await _client.PostAsync(BaseRoute, ToJson(ValidCreatePayload("Admin CRUD Test")));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var id = JsonDocument.Parse(await createResp.Content.ReadAsStringAsync())
            .RootElement.GetProperty("data").GetProperty("id").GetString()!;

        // Update
        var updateResp = await _client.PutAsync($"{BaseRoute}/{id}", ToJson(ValidUpdatePayload("Admin CRUD Updated")));
        Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

        // Delete
        var deleteResp = await _client.DeleteAsync($"{BaseRoute}/{id}");
        Assert.Equal(HttpStatusCode.OK, deleteResp.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A16 — RBAC: Client cannot PUT
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A16_Update_ClientRole_Returns403()
    {
        SetClientToken();
        var response = await _client.PutAsync(
            $"{BaseRoute}/{Guid.NewGuid()}",
            ToJson(ValidUpdatePayload()));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A17 — RBAC: Client cannot DELETE
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A17_Delete_ClientRole_Returns403()
    {
        SetClientToken();
        var response = await _client.DeleteAsync($"{BaseRoute}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // A18 — RBAC: Provider cannot DELETE
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task A18_Delete_ProviderRole_Returns403()
    {
        SetProviderToken();
        var response = await _client.DeleteAsync($"{BaseRoute}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // C01 — Client: GET all active categories (no token needed)
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task C01_GetAll_NoToken_Returns200()
    {
        ClearToken();
        var response = await _client.GetAsync(BaseRoute);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // C02 — Client: GET single active category (no token needed)
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task C02_GetById_NoToken_Returns200()
    {
        var id = await CreateCategoryAndGetId("Public Category");

        ClearToken();
        var response = await _client.GetAsync($"{BaseRoute}/categories/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ════════════════════════════════════════════════════════════════════════
    // S01 — Contract: response envelope is consistent on success
    // Envelope: { statusCode, isSuccess, message, data, errors }
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task S01_ResponseEnvelope_IsConsistentOnSuccess()
    {
        ClearToken();
        var response = await _client.GetAsync(BaseRoute);
        var body     = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.True(body.TryGetProperty("isSuccess",  out _), "Missing isSuccess");
        Assert.True(body.TryGetProperty("message",    out _), "Missing message");
        Assert.True(body.TryGetProperty("statusCode", out _), "Missing statusCode");
        Assert.True(body.TryGetProperty("data",       out _), "Missing data");
    }

    // ════════════════════════════════════════════════════════════════════════
    // S02 — Contract: response envelope on error is consistent
    // GlobalErrorHandlerMiddleware returns same ApiResponse wrapper on exceptions
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task S02_ResponseEnvelope_IsConsistentOnError()
    {
        ClearToken();
        var response = await _client.GetAsync($"{BaseRoute}/categories/{Guid.NewGuid()}");
        var body     = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.True(body.TryGetProperty("isSuccess",  out var isSuccess));
        Assert.False(isSuccess.GetBoolean(), "isSuccess must be false on error");
        Assert.True(body.TryGetProperty("statusCode", out _), "Missing statusCode");
        Assert.True(body.TryGetProperty("message",    out _), "Missing message");
    }

    // ════════════════════════════════════════════════════════════════════════
    // S03 — Contract: created category Id is a valid GUID (not int)
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task S03_Create_IdIsGuid()
    {
        SetAdminToken();
        var response = await _client.PostAsync(BaseRoute, ToJson(ValidCreatePayload("Guid Check")));
        var id = JsonDocument.Parse(await response.Content.ReadAsStringAsync())
            .RootElement.GetProperty("data").GetProperty("id").GetString();

        Assert.True(Guid.TryParse(id, out _), $"Expected GUID but got: {id}");
    }

    // ════════════════════════════════════════════════════════════════════════
    // S04 — Contract: Content-Type is application/json
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task S04_Response_ContentTypeIsJson()
    {
        ClearToken();
        var response = await _client.GetAsync(BaseRoute);

        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    // ════════════════════════════════════════════════════════════════════════
    // S05 — Edge case: empty DB returns 200 with empty data, not 404
    // ════════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task S05_GetAll_EmptyDb_Returns200NotNull()
    {
        ClearToken();
        var response = await _client.GetAsync(BaseRoute);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
        // data should not be null even when empty
        Assert.NotEqual(JsonValueKind.Null, body.GetProperty("data").ValueKind);
    }
}