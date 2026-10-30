using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Files.GetFileConfig.Domain;
using PolyBucket.Api.Features.Files.GetFileConfig.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Files.GetFileConfig.Http;

public class GetFileConfigControllerTests
{
    private readonly Mock<IGetFileConfigService> _service = new();
    private readonly GetFileConfigController _controller;

    public GetFileConfigControllerTests()
    {
        _controller = new GetFileConfigController(_service.Object);
    }

    [Fact(DisplayName = "When the controller is constructed, it is not null.")]
    public void Controller_IsCreated()
    {
        _controller.ShouldNotBeNull();
    }
}
