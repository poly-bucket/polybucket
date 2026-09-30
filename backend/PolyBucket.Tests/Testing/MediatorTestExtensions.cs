using System;
using System.Threading;
using MediatR;
using Moq;

namespace PolyBucket.Tests.Testing;

public static class MediatorTestExtensions
{
    public static void SetupSend<TRequest, TResponse>(
        this Mock<IMediator> mediator,
        TResponse response)
        where TRequest : IRequest<TResponse>
    {
        mediator
            .Setup(m => m.Send(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }

    public static void SetupSend<TRequest, TResponse>(
        this Mock<IMediator> mediator,
        Func<TRequest, TResponse> factory)
        where TRequest : IRequest<TResponse>
    {
        mediator
            .Setup(m => m.Send(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TRequest req, CancellationToken _) => factory(req));
    }

    public static void SetupSendThrows<TRequest, TResponse>(
        this Mock<IMediator> mediator,
        Exception exception)
        where TRequest : IRequest<TResponse>
    {
        mediator
            .Setup(m => m.Send(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
    }
}
