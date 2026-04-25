using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SoftBridge.Web;
using Xunit;

namespace SoftBridge.API.Tests.Hubs;

/// <summary>
/// Real-services integration tests for ChatHub.
/// Uses the full DI graph with InMemory database and no service mocks.
/// </summary>
public class ChatHubRealIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;

    // JWT settings must match appsettings.json
    private const string JwtKey = "7zzJSeUi5eL3PfoQt7XieaSsbzIZ3CS5juJNbW4kouJx6Bpdkt";
    private const string JwtIssuer = "http://localhost:5000";
    private const string JwtAudience = "SoftBridge";

    private static readonly string DbName = "ChatHubRealTestDb_" + Guid.NewGuid();

    public ChatHubRealIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
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
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    // ════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════════════════════

    private static string GenerateJwt(string userId, string role = "Client")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
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

    private HubConnection CreateConnection(string? accessToken)
    {
        var hubUri = new Uri(_factory.Server.BaseAddress, "/chatHub");

        return new HubConnectionBuilder()
            .WithUrl(hubUri, options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();

                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken)!;
                }
            })
            .Build();
    }

    private static async Task DisposeConnectionAsync(HubConnection? connection)
    {
        if (connection is null)
            return;

        try
        {
            if (connection.State != HubConnectionState.Disconnected)
            {
                await connection.StopAsync();
            }
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // REAL SERVICE + INMEMORY SCENARIOS
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CHR01_AuthorizedUser_CanConnectToChatHub()
    {
        var connection = CreateConnection(GenerateJwt("chat-real-user-01"));

        try
        {
            await connection.StartAsync();
            Assert.Equal(HubConnectionState.Connected, connection.State);
        }
        finally
        {
            await DisposeConnectionAsync(connection);
        }
    }

    [Fact]
    public async Task CHR02_NoToken_ConnectionRejected()
    {
        var connection = CreateConnection(null);

        try
        {
            await Assert.ThrowsAnyAsync<Exception>(() => connection.StartAsync());
            Assert.NotEqual(HubConnectionState.Connected, connection.State);
        }
        finally
        {
            await DisposeConnectionAsync(connection);
        }
    }

    [Fact]
    public async Task CHR03_AuthorizedUser_CanJoinChatGroup()
    {
        var connection = CreateConnection(GenerateJwt("chat-real-user-02", "Provider"));

        try
        {
            await connection.StartAsync();
            await connection.InvokeAsync("JoinChat", Guid.NewGuid());
            Assert.Equal(HubConnectionState.Connected, connection.State);
        }
        finally
        {
            await DisposeConnectionAsync(connection);
        }
    }

    [Fact]
    public async Task CHR04_SendMessage_NonExistentRequest_Throws()
    {
        var connection = CreateConnection(GenerateJwt("chat-real-user-03"));

        try
        {
            await connection.StartAsync();

            await Assert.ThrowsAnyAsync<Exception>(
                () => connection.InvokeAsync("SendMessage", Guid.NewGuid(), "real integration message"));
        }
        finally
        {
            await DisposeConnectionAsync(connection);
        }
    }
}
