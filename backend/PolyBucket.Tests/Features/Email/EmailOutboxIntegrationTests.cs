using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Email.Templates;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Email.Domain;
using PolyBucket.Api.Features.Email.Repository;
using PolyBucket.Api.Features.SystemSettings.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Email;

[Collection("TestCollection")]
public class EmailOutboxIntegrationTests : BaseIntegrationTest
{
    public EmailOutboxIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    private async Task EnableCapturedEmailAsync()
    {
        DbContext.SystemSettings.AddRange(
            new SystemSetting { Key = SystemSettingKeys.EmailTransport, Value = nameof(EmailTransportKind.Log) },
            new SystemSetting { Key = SystemSettingKeys.EmailFromAddress, Value = "noreply@polybucket.test" },
            new SystemSetting { Key = SystemSettingKeys.EmailPublicBaseUrl, Value = "https://models.polybucket.test" });
        await DbContext.SaveChangesAsync();
        ServiceScope.ServiceProvider.GetRequiredService<IEmailSettingsResolver>().Invalidate();
        Factory.EmailTransport.Clear();
    }

    private async Task SeedPendingMessagesAsync(int count, DateTime nextAttemptAt)
    {
        for (var i = 0; i < count; i++)
        {
            DbContext.EmailMessages.Add(new EmailMessage
            {
                TemplateKey = EmailTemplateKey.Welcome,
                Recipient = $"user{i}@polybucket.test",
                ModelJson = "{\"username\":\"maker\"}",
                Status = EmailMessageStatus.Pending,
                NextAttemptAt = nextAttemptAt,
                CreatedAt = DateTime.UtcNow
            });
        }

        await DbContext.SaveChangesAsync();
    }

    private async Task<IReadOnlyList<EmailMessage>> ClaimInOwnScopeAsync(int batchSize, TimeSpan lockDuration, DateTime now)
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEmailOutboxRepository>();
        return await repository.ClaimBatchAsync(batchSize, lockDuration, now);
    }

    [Fact(DisplayName = "When two dispatchers claim at the same time, SKIP LOCKED ensures no message is claimed twice.")]
    public async Task ClaimBatch_Concurrent_ClaimsDisjointSets()
    {
        // Arrange
        await ResetStateAsync();
        await SeedPendingMessagesAsync(30, DateTime.UtcNow.AddMinutes(-1));
        var now = DateTime.UtcNow;

        // Act
        var claims = await Task.WhenAll(
            ClaimInOwnScopeAsync(20, TimeSpan.FromMinutes(2), now),
            ClaimInOwnScopeAsync(20, TimeSpan.FromMinutes(2), now));

        // Assert
        var allIds = claims.SelectMany(c => c.Select(m => m.Id)).ToList();
        allIds.Count.ShouldBe(allIds.Distinct().Count());
        allIds.Count.ShouldBe(30);
        claims.SelectMany(c => c).ShouldAllBe(m => m.Status == EmailMessageStatus.Sending && m.Attempts == 1);
    }

    [Fact(DisplayName = "When a message is scheduled in the future, it is not claimed until its attempt time.")]
    public async Task ClaimBatch_FutureMessage_IsNotClaimed()
    {
        // Arrange
        await ResetStateAsync();
        await SeedPendingMessagesAsync(1, DateTime.UtcNow.AddMinutes(10));

        // Act
        var claimed = await ClaimInOwnScopeAsync(10, TimeSpan.FromMinutes(2), DateTime.UtcNow);

        // Assert
        claimed.ShouldBeEmpty();
    }

    [Fact(DisplayName = "When a dispatcher crashes mid-send, the message is reclaimed after its lock expires.")]
    public async Task ClaimBatch_ExpiredLock_IsReclaimed()
    {
        // Arrange
        await ResetStateAsync();
        await SeedPendingMessagesAsync(1, DateTime.UtcNow.AddMinutes(-1));
        var first = await ClaimInOwnScopeAsync(10, TimeSpan.FromSeconds(30), DateTime.UtcNow);

        // Act
        var whileLocked = await ClaimInOwnScopeAsync(10, TimeSpan.FromSeconds(30), DateTime.UtcNow);
        var afterExpiry = await ClaimInOwnScopeAsync(10, TimeSpan.FromSeconds(30), DateTime.UtcNow.AddMinutes(1));

        // Assert
        first.ShouldHaveSingleItem();
        whileLocked.ShouldBeEmpty();
        afterExpiry.ShouldHaveSingleItem().Attempts.ShouldBe(2);
    }

    [Fact(DisplayName = "When a user requests a password reset, the reset email is queued, delivered by the dispatcher, and its token is scrubbed from the outbox.")]
    public async Task ForgotPassword_QueuesAndDeliversResetEmail()
    {
        // Arrange
        await ResetStateAsync();
        await EnableCapturedEmailAsync();
        var user = await CreateTestUser("reset-me@polybucket.test");
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/forgot-password", new { email = user.Email });
        using var scope = Factory.Services.CreateScope();
        var dispatched = await scope.ServiceProvider.GetRequiredService<IEmailDispatcher>().DispatchPendingAsync();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        dispatched.ShouldBe(1);
        var email = Factory.EmailTransport.SentTo(user.Email).ShouldHaveSingleItem();
        email.HtmlBody.ShouldContain("https://models.polybucket.test/reset-password?token=");
        email.TextBody.ShouldContain("https://models.polybucket.test/reset-password?token=");

        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
        var stored = await context.EmailMessages.AsNoTracking().SingleAsync();
        stored.Status.ShouldBe(EmailMessageStatus.Sent);
        stored.ModelJson.ShouldNotContain("token=");
    }

    [Fact(DisplayName = "When a user requests a password reset for an unknown address, the response is identical and no email is queued.")]
    public async Task ForgotPassword_UnknownEmail_QueuesNothing()
    {
        // Arrange
        await ResetStateAsync();
        await EnableCapturedEmailAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/forgot-password", new { email = "nobody@polybucket.test" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await DbContext.EmailMessages.AsNoTracking().CountAsync()).ShouldBe(0);
    }

    [Fact(DisplayName = "When delivery fails transiently, the message is rescheduled and delivered on a later dispatch.")]
    public async Task Dispatch_TransientFailure_IsRetried()
    {
        // Arrange
        await ResetStateAsync();
        await EnableCapturedEmailAsync();
        await SeedPendingMessagesAsync(1, DateTime.UtcNow.AddMinutes(-1));
        Factory.EmailTransport.NextFailure = new EmailDeliveryException("421 try again later", isTransient: true);
        using var scope = Factory.Services.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IEmailDispatcher>();
        var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();

        // Act
        await dispatcher.DispatchPendingAsync();
        var afterFailure = await context.EmailMessages.AsNoTracking().SingleAsync();
        await context.EmailMessages.ExecuteUpdateAsync(s => s.SetProperty(m => m.NextAttemptAt, DateTime.UtcNow.AddMinutes(-1)));
        await dispatcher.DispatchPendingAsync();
        var afterRetry = await context.EmailMessages.AsNoTracking().SingleAsync();

        // Assert
        afterFailure.Status.ShouldBe(EmailMessageStatus.Failed);
        afterFailure.LastError.ShouldBe("421 try again later");
        afterFailure.NextAttemptAt.ShouldBeGreaterThan(DateTime.UtcNow);
        afterRetry.Status.ShouldBe(EmailMessageStatus.Sent);
        afterRetry.Attempts.ShouldBe(2);
        Factory.EmailTransport.Sent.ShouldHaveSingleItem();
    }
}
