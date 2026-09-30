using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Email.Templates;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.ModelModeration.ApproveModel.Domain;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.Notifications.Domain;
using PolyBucket.Api.Features.SystemSettings.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Notifications;

[Collection("TestCollection")]
public class NotificationsIntegrationTests : BaseIntegrationTest
{
    private const string Password = "Password123!";

    public NotificationsIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    private async Task EnableEmailAsync()
    {
        DbContext.SystemSettings.AddRange(
            new SystemSetting { Key = SystemSettingKeys.EmailTransport, Value = nameof(EmailTransportKind.Log) },
            new SystemSetting { Key = SystemSettingKeys.EmailFromAddress, Value = "noreply@polybucket.test" },
            new SystemSetting { Key = SystemSettingKeys.EmailPublicBaseUrl, Value = "https://models.polybucket.test" },
            new SystemSetting { Key = SystemSettingKeys.EmailRequireVerification, Value = "false" });
        await DbContext.SaveChangesAsync();
        ServiceScope.ServiceProvider.GetRequiredService<IEmailSettingsResolver>().Invalidate();
    }

    private async Task<User> CreateVerifiedUserAsync(string email)
    {
        var user = await CreateTestUser(email, Password);
        user.EmailVerifiedAt = user.CreatedAt;
        await DbContext.SaveChangesAsync();
        return user;
    }

    private async Task<HttpClient> ClientForAsync(string email)
    {
        var client = Factory.CreateClient();
        var token = await GetAuthTokenWithClient(client, email, Password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task ApproveAsync(Guid modelId, Guid moderatorId)
    {
        using var scope = Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IApproveModelService>().ApproveAsync(modelId, moderatorId, null, null);
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.Clone();
    }

    [Fact(DisplayName = "Approving a model notifies the author in-app and queues a notification email in the same transaction.")]
    public async Task ApproveModel_CreatesNotificationAndOutboxEmail()
    {
        // Arrange
        await ResetStateAsync();
        await EnableEmailAsync();
        var author = await CreateVerifiedUserAsync("approved-author@polybucket.test");
        var moderator = await CreateVerifiedUserAsync("approving-mod@polybucket.test");
        var model = await ModelFactory.CreateTestModel("Benchy", "desc", author.Id);
        DbContext.ModelModerationRecords.Add(new ModelModerationRecord { Id = Guid.NewGuid(), ModelId = model.Id, Status = ModelModerationStatus.Pending });
        await DbContext.SaveChangesAsync();

        // Act
        await ApproveAsync(model.Id, moderator.Id);

        // Assert
        var notification = await DbContext.Notifications.AsNoTracking().SingleAsync(n => n.UserId == author.Id);
        notification.Type.ShouldBe(NotificationType.ModelApproved);
        notification.ActorUserId.ShouldBe(moderator.Id);
        notification.ActionUrl.ShouldBe($"/models/{model.Id}");
        notification.IsRead.ShouldBeFalse();

        var email = await DbContext.EmailMessages.AsNoTracking().SingleAsync(m => m.Recipient == author.Email);
        email.TemplateKey.ShouldBe(EmailTemplateKey.Notification);
        email.IdempotencyKey.ShouldBe($"notification:{notification.Id}");
        email.ModelJson.ShouldContain("https://models.polybucket.test/models/");
    }

    [Fact(DisplayName = "An author sees the approval notification, the unread count drops after marking it read, and mark-all clears the rest.")]
    public async Task NotificationEndpoints_ListCountAndMarkRead()
    {
        // Arrange
        await ResetStateAsync();
        var author = await CreateVerifiedUserAsync("reader@polybucket.test");
        var moderator = await CreateVerifiedUserAsync("reader-mod@polybucket.test");
        var first = await ModelFactory.CreateTestModel("First", "desc", author.Id);
        var second = await ModelFactory.CreateTestModel("Second", "desc", author.Id);
        DbContext.ModelModerationRecords.AddRange(
            new ModelModerationRecord { Id = Guid.NewGuid(), ModelId = first.Id, Status = ModelModerationStatus.Pending },
            new ModelModerationRecord { Id = Guid.NewGuid(), ModelId = second.Id, Status = ModelModerationStatus.Pending });
        await DbContext.SaveChangesAsync();
        await ApproveAsync(first.Id, moderator.Id);
        await ApproveAsync(second.Id, moderator.Id);
        var client = await ClientForAsync(author.Email);

        // Act
        var list = await ReadJsonAsync(await client.GetAsync("/api/notifications?page=1&pageSize=10"));
        var firstId = list.GetProperty("items")[0].GetProperty("id").GetString();
        var markOne = await client.PostAsync($"/api/notifications/{firstId}/read", null);
        var countAfterOne = await ReadJsonAsync(await client.GetAsync("/api/notifications/unread-count"));
        var markAll = await ReadJsonAsync(await client.PostAsync("/api/notifications/read-all", null));
        var countAfterAll = await ReadJsonAsync(await client.GetAsync("/api/notifications/unread-count"));

        // Assert
        list.GetProperty("totalCount").GetInt32().ShouldBe(2);
        list.GetProperty("unreadCount").GetInt32().ShouldBe(2);
        list.GetProperty("items")[0].GetProperty("type").GetString().ShouldBe(nameof(NotificationType.ModelApproved));
        markOne.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        countAfterOne.GetProperty("count").GetInt32().ShouldBe(1);
        markAll.GetProperty("updated").GetInt32().ShouldBe(1);
        countAfterAll.GetProperty("count").GetInt32().ShouldBe(0);
    }

    [Fact(DisplayName = "A user cannot mark another user's notification as read.")]
    public async Task MarkRead_OtherUsersNotification_ReturnsNotFound()
    {
        // Arrange
        await ResetStateAsync();
        var author = await CreateVerifiedUserAsync("owner-note@polybucket.test");
        var moderator = await CreateVerifiedUserAsync("owner-note-mod@polybucket.test");
        var intruder = await CreateVerifiedUserAsync("intruder@polybucket.test");
        var model = await ModelFactory.CreateTestModel("Owned", "desc", author.Id);
        DbContext.ModelModerationRecords.Add(new ModelModerationRecord { Id = Guid.NewGuid(), ModelId = model.Id, Status = ModelModerationStatus.Pending });
        await DbContext.SaveChangesAsync();
        await ApproveAsync(model.Id, moderator.Id);
        var notificationId = await DbContext.Notifications.Where(n => n.UserId == author.Id).Select(n => n.Id).SingleAsync();
        var client = await ClientForAsync(intruder.Email);

        // Act
        var response = await client.PostAsync($"/api/notifications/{notificationId}/read", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await DbContext.Notifications.AsNoTracking().SingleAsync(n => n.Id == notificationId)).IsRead.ShouldBeFalse();
    }

    [Fact(DisplayName = "Liking, unliking, and liking again notifies the author once and never by email.")]
    public async Task LikeModel_RepeatedLikes_NotifyOnceInAppOnly()
    {
        // Arrange
        await ResetStateAsync();
        await EnableEmailAsync();
        var author = await CreateVerifiedUserAsync("liked-author@polybucket.test");
        var fan = await CreateVerifiedUserAsync("fan@polybucket.test");
        var model = await ModelFactory.CreateTestModel("Popular", "desc", author.Id);
        model.IsPublic = true;
        await DbContext.SaveChangesAsync();
        var client = await ClientForAsync(fan.Email);

        // Act
        var like = await client.PostAsync($"/api/models/{model.Id}/like", null);
        await client.DeleteAsync($"/api/models/{model.Id}/like");
        await client.PostAsync($"/api/models/{model.Id}/like", null);

        // Assert
        like.IsSuccessStatusCode.ShouldBeTrue();
        var notifications = await DbContext.Notifications.AsNoTracking().Where(n => n.UserId == author.Id).ToListAsync();
        notifications.Count.ShouldBe(1);
        notifications[0].Type.ShouldBe(NotificationType.ModelLiked);
        notifications[0].Title.ShouldContain(fan.Username);
        (await DbContext.EmailMessages.AnyAsync(m => m.Recipient == author.Email)).ShouldBeFalse();
    }

    [Fact(DisplayName = "Commenting on a model notifies its author, and replying notifies the parent commenter.")]
    public async Task CommentAndReply_NotifyAuthorAndParentCommenter()
    {
        // Arrange
        await ResetStateAsync();
        var author = await CreateVerifiedUserAsync("comment-author@polybucket.test");
        var commenter = await CreateVerifiedUserAsync("commenter@polybucket.test");
        var replier = await CreateVerifiedUserAsync("replier@polybucket.test");
        var model = await ModelFactory.CreateTestModel("Discussed", "desc", author.Id);
        model.IsPublic = true;
        await DbContext.SaveChangesAsync();
        var commenterClient = await ClientForAsync(commenter.Email);
        var replierClient = await ClientForAsync(replier.Email);

        // Act
        var created = await commenterClient.PostAsync("/api/comments", JsonContent(new { target = new { targetId = model.Id, targetType = "Model" }, content = "Great print!" }));
        var parentId = (await ReadJsonAsync(created)).GetProperty("id").GetString();
        var reply = await replierClient.PostAsync("/api/comments", JsonContent(new { target = new { targetId = model.Id, targetType = "Model" }, content = "Agreed", parentCommentId = parentId }));

        // Assert
        created.IsSuccessStatusCode.ShouldBeTrue();
        reply.IsSuccessStatusCode.ShouldBeTrue();
        var authorTypes = await DbContext.Notifications.AsNoTracking().Where(n => n.UserId == author.Id).Select(n => n.Type).ToListAsync();
        authorTypes.ShouldBe(new[] { NotificationType.CommentAdded, NotificationType.CommentAdded }, ignoreOrder: true);
        var commenterNotification = await DbContext.Notifications.AsNoTracking().SingleAsync(n => n.UserId == commenter.Id);
        commenterNotification.Type.ShouldBe(NotificationType.CommentReplied);
        commenterNotification.ActionUrl.ShouldBe($"/models/{model.Id}#comments");
        (await DbContext.Notifications.AnyAsync(n => n.UserId == replier.Id)).ShouldBeFalse();
    }

    private static StringContent JsonContent(object body) =>
        new(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json");
}
