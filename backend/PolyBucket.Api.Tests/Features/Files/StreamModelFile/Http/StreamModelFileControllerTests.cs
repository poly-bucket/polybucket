using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Files.StreamModelFile.Domain;
using PolyBucket.Api.Features.Files.StreamModelFile.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Files.StreamModelFile.Http;

public class StreamModelFileControllerTests
{
    private readonly Mock<IStreamModelFileService> _service = new();
    private readonly StreamModelFileController _controller;

    public StreamModelFileControllerTests()
    {
        _controller = new StreamModelFileController(_service.Object);
    }

    [Fact(DisplayName = "When the controller is constructed, it is not null.")]
    public void Controller_IsCreated()
    {
        _controller.ShouldNotBeNull();
    }
}