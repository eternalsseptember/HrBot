using System.Net;
using System.Text;
using HrBot.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types;

namespace HrBot.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                ["Configuration:BotToken"] = "123456789:ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcd",
                ["Configuration:WebHookAddress"] = "https://example.test/webhook",
                ["Configuration:RepostToChannelId"] = "-1001234567890",
                ["Configuration:RepostOnlyFromChatIdsEnabled"] = "false",
                ["Configuration:TechnicalChatId"] = "0"
            };
            cfg.AddInMemoryCollection(inMemorySettings!);
        });

        builder.ConfigureServices(services =>
        {
            // Replace IVacancyReposter to avoid heavy logic during tests
            var reposterMock = new Mock<IVacancyReposter>();
            reposterMock.Setup(r => r.TryRepost(It.IsAny<Message>())).Returns(Task.CompletedTask);
            reposterMock.Setup(r => r.TryEdit(It.IsAny<Message>())).Returns(Task.CompletedTask);

            var descriptorReposter = services.SingleOrDefault(d => d.ServiceType == typeof(IVacancyReposter));
            if (descriptorReposter != null)
                services.Remove(descriptorReposter);
            services.AddSingleton(reposterMock.Object);
        });
    }
}

public class IntegrationTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public IntegrationTest(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HrUpdate_Post_ReturnsOk()
    {
        var testBody = """
                        {"update_id":1231223123,
                       "message":{"message_id":757,"from":{"id":123123123,"is_bot":false,"first_name":"Dr.","last_name":"Redacted","username":"redacted","is_premium":true},"chat":{"id":-123123123123,"title":"Redacted 14","username":"redacted14","type":"supergroup"},"date":1758486604,"text":"\u044d\u043c, \u0442\u0435\u0441\u0442?"}}
                       """;

        using var content = new StringContent(testBody, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/hrupdate", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}