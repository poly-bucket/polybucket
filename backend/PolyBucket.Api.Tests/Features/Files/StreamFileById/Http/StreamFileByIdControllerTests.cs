using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Files.StreamFileById.Domain;
using PolyBucket.Api.Features.Files.StreamFileById.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Files.StreamFileById.Http;

public class StreamFileByIdControllerTests
{
    private readonly Mock<IStreamFileByIdService> _service = new();
    private readonly StreamFileByIdController _controller;

    public StreamFileByIdControllerTests()
    {
        _controller = new StreamFileByIdController(_service.Object);
    }

    [Fact(DisplayName = "When the controller is constructed, it is not null.")]
    public void Controller_IsCreated()
    {
        _controller.ShouldNotBeNull();
    }
}
