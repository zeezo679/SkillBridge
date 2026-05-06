using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using SoftBridge.Abstraction.IServicesContract.Chat;
using SoftBridge.Shared.Common.Dto.Chat;
using SoftBridge.Web;
using Xunit;

namespace SoftBridge.API.Tests.Hubs;

/// <summary>
/// Integration tests for ChatHub.
/// Uses WebApplicationFactory to run the real ASP.NET Core pipeline.
/// </summary>
public class ChatHubTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
	private readonly WebApplicationFactory<Program> _factory;
	private readonly Mock<IChatService> _chatServiceMock;
	private HubConnection? _senderConnection;
	private HubConnection? _receiverConnection;

	// ── JWT settings — must match appsettings.json exactly ────────────────
	private const string JwtKey = "7zzJSeUi5eL3PfoQt7XieaSsbzIZ3CS5juJNbW4kouJx6Bpdkt";
	private const string JwtIssuer = "http://localhost:5000";
	private const string JwtAudience = "SoftBridge";

	// ── Shared DB name across all tests in this class ─────────────────────
	private static readonly string DbName = "ChatHubTestDb_" + Guid.NewGuid();

	public ChatHubTests(WebApplicationFactory<Program> factory)
	{
		_chatServiceMock = new Mock<IChatService>();

		_chatServiceMock
			.Setup(s => s.SaveMessageAsync(It.IsAny<string>(), It.IsAny<SendMessageDto>()))
			.ReturnsAsync((string senderId, SendMessageDto dto) => new MessageDto
			{
				Id = Guid.NewGuid(),
				SenderId = senderId,
				SenderName = "Integration User",
				RequestId = dto.RequestId,
				Content = dto.Content,
				SentAt = DateTime.UtcNow,
				IsRead = false
			});

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

			builder.ConfigureServices(services =>
			{
				services.RemoveAll<IChatService>();
				services.AddSingleton(_chatServiceMock.Object);
			});
		});
	}

	public Task InitializeAsync() => Task.CompletedTask;

	public async Task DisposeAsync()
	{
		await DisposeConnectionAsync(_senderConnection);
		await DisposeConnectionAsync(_receiverConnection);
	}

	// ════════════════════════════════════════════════════════════════════════
	// HELPERS
	// ════════════════════════════════════════════════════════════════════════

	private static string GenerateJwt(string userId)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, userId),
			new Claim(ClaimTypes.Role, "Client")
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
	// CH01 — Authorized user can connect to ChatHub
	// ════════════════════════════════════════════════════════════════════════
	[Fact]
	public async Task CH01_AuthorizedUser_CanConnectToChatHub()
	{
		var token = GenerateJwt("chat-user-01");
		var connection = CreateConnection(token);

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

	// ════════════════════════════════════════════════════════════════════════
	// CH02 — Missing token cannot connect to ChatHub
	// ════════════════════════════════════════════════════════════════════════
	[Fact]
	public async Task CH02_NoToken_ConnectionRejected()
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

	// ════════════════════════════════════════════════════════════════════════
	// CH03 — SendMessage broadcasts to all members in the same chat group
	// ════════════════════════════════════════════════════════════════════════
	[Fact]
	public async Task CH03_SendMessage_BroadcastsToJoinedGroupMembers()
	{
		var requestId = Guid.NewGuid();
		const string content = "hello from integration test";
		const string senderUserId = "chat-sender-01";

		_senderConnection = CreateConnection(GenerateJwt(senderUserId));
		_receiverConnection = CreateConnection(GenerateJwt("chat-receiver-01"));

		var senderReceived = new TaskCompletionSource<MessageDto>(TaskCreationOptions.RunContinuationsAsynchronously);
		var receiverReceived = new TaskCompletionSource<MessageDto>(TaskCreationOptions.RunContinuationsAsynchronously);

		using var senderSubscription = _senderConnection.On<MessageDto>("ReceiveMessage", message =>
		{
			senderReceived.TrySetResult(message);
		});

		using var receiverSubscription = _receiverConnection.On<MessageDto>("ReceiveMessage", message =>
		{
			receiverReceived.TrySetResult(message);
		});

		await Task.WhenAll(_senderConnection.StartAsync(), _receiverConnection.StartAsync());

		await _senderConnection.InvokeAsync("JoinChat", requestId);
		await _receiverConnection.InvokeAsync("JoinChat", requestId);

		await _senderConnection.InvokeAsync("SendMessage", requestId, content);

		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
		var senderMessage = await senderReceived.Task.WaitAsync(cts.Token);
		var receiverMessage = await receiverReceived.Task.WaitAsync(cts.Token);

		Assert.Equal(requestId, senderMessage.RequestId);
		Assert.Equal(content, senderMessage.Content);
		Assert.Equal(senderUserId, senderMessage.SenderId);

		Assert.Equal(requestId, receiverMessage.RequestId);
		Assert.Equal(content, receiverMessage.Content);
		Assert.Equal(senderUserId, receiverMessage.SenderId);

		_chatServiceMock.Verify(
			s => s.SaveMessageAsync(
				senderUserId,
				It.Is<SendMessageDto>(d => d.RequestId == requestId && d.Content == content)),
			Times.Once);
	}
}
