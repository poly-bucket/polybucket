using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Comments.Queries;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments.GetCommentsForModel;

public class GetCommentsForModelControllerTests
{
    private readonly Mock<ICommentsPlugin> _plugin = new();
    private readonly GetCommentsForModelController _controller;

    public GetCommentsForModelControllerTests() => _controller = new GetCommentsForModelController(_plugin.Object).WithUser(Guid.NewGuid());

    [Fact(DisplayName = "When comments exist for a model, GetCommentsForModel returns Ok with the list.")]
    public async Task GetCommentsForModel_ReturnsOk()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var comments = new List<Comment>
        {
            new()
            {
                Content = "A",
                Model = new Model { Name = "M" },
                Author = new User { Username = "u", Email = "e@e.com", Salt = "s", PasswordHash = "h" }
            }
        };
        _plugin.Setup(p => p.GetCommentsForModelAsync(modelId)).ReturnsAsync(comments);

        // Act
        var result = await _controller.GetCommentsForModel(modelId);

        // Assert
        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(comments);
    }
}
