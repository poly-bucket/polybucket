using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;

public interface IModelPreviewSignal
{
    void Notify();
    Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken);
}

public class ModelPreviewSignal : IModelPreviewSignal
{
    private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
    {
        FullMode = BoundedChannelFullMode.DropWrite
    });

    public void Notify()
    {
        _channel.Writer.TryWrite(true);
    }

    public async Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);
        try
        {
            await _channel.Reader.ReadAsync(timeoutSource.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
        }
    }
}
