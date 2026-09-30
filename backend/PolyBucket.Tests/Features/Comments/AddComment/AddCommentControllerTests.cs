using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Comments.Commands;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.AddComment;

public class AddCommentControllerTests
{
    private readonly Mock<ICommentsPlugin> _plugin = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _modelId = Guid.NewGuid();

    private AddCommentController CreateController(Guid? userId) =>
        new AddCommentController(_plugin.Object).WithUser(userId);

    [Fact(DisplayName = "When a signed-in user adds a comment, AddComment returns Ok with the comment.")]
    public async Task AddComment_Valid_ReturnsOk()
    {
        // Arrange
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = "Nice print",
            Model = new Model { Name = "M" },
            Author = new User { Username = "u", Email = "e@e.com", Salt = "s", PasswordHash = "h" }
        };
        _plugin.Setup(p => p.AddCommentAsync(_modelId, _userId, "Nice print")).ReturnsAsync(comment);

        // Act
        var result = await CreateController(_userId).AddComment(_modelId, new PolyBucket.Api.Features.Comments.Commands.AddCommentRequest { Content = "Nice print" });

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(comment);
    }

    [Fact(DisplayName = "When the user id claim is missing, AddComment throws UnauthorizedAccessException.")]
    public async Task AddComment_NoUser_Throws()
    {
        // Act & Assert
        await Should.ThrowAsync<UnauthorizedAccessException>(() =>
            CreateController(null).AddComment(_modelId, new PolyBucket.Api.Features.Comments.Commands.AddCommentRequest { Content = "x" }));
    }
}
