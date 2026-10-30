using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Files.GetSupportedExtensions.Domain;
using PolyBucket.Api.Features.Files.GetSupportedExtensions.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Files.GetSupportedExtensions.Http;

public class GetSupportedExtensionsControllerTests
{
    private readonly Mock<IGetSupportedExtensionsService> _service = new();
    private readonly GetSupportedExtensionsController _controller;

    public GetSupportedExtensionsControllerTests()
    {
        _controller = new GetSupportedExtensionsController(_service.Object);
    }

    [Fact(DisplayName = "When the controller is constructed, it is not null.")]
    public void Controller_IsCreated()
    {
        _controller.ShouldNotBeNull();
    }
}
