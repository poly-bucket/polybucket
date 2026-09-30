using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Xunit;

namespace PolyBucket.Tests.Email;

[CollectionDefinition("MailpitCollection")]
public class MailpitCollection : ICollectionFixture<MailpitFixture>
{
}

public class MailpitFixture : IAsyncLifetime
{
    public const string Username = "polybucket";
    public const string Password = "mailpit-secret";
    private const ushort SmtpPort = 1025;
    private const ushort HttpPort = 8025;

    private IContainer? _container;

    public string Host => _container!.Hostname;
    public int Port => _container!.GetMappedPublicPort(SmtpPort);
    public HttpClient Api { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _container = new ContainerBuilder()
            .WithImage("axllent/mailpit:v1.21")
            .WithEnvironment("MP_SMTP_AUTH", $"{Username}:{Password}")
            .WithEnvironment("MP_SMTP_AUTH_ALLOW_INSECURE", "true")
            .WithPortBinding(SmtpPort, true)
            .WithPortBinding(HttpPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilInternalTcpPortIsAvailable(SmtpPort)
                .UntilHttpRequestIsSucceeded(r => r.ForPort(HttpPort).ForPath("/livez")))
            .Build();

        using var startCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        await _container.StartAsync(startCts.Token).ConfigureAwait(false);

        Api = new HttpClient
        {
            BaseAddress = new Uri($"http://{_container.Hostname}:{_container.GetMappedPublicPort(HttpPort)}")
        };
    }

    public async Task ClearAsync()
    {
        await Api.DeleteAsync("/api/v1/messages");
    }

    public async Task<JsonElement> GetMessagesAsync()
    {
        return await Api.GetFromJsonAsync<JsonElement>("/api/v1/messages");
    }

    public async Task<JsonElement> GetMessageAsync(string id)
    {
        return await Api.GetFromJsonAsync<JsonElement>($"/api/v1/message/{id}");
    }

    public async Task DisposeAsync()
    {
        Api?.Dispose();
        if (_container is not null)
        {
            await _container.DisposeAsync().ConfigureAwait(false);
        }
    }
}
