using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace PolyBucket.Api.Common.Email;

public static class EmailMetrics
{
    public const string MeterName = "PolyBucket.Email";

    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> Sent = Meter.CreateCounter<long>("email.sent", description: "Emails delivered to the transport");
    private static readonly Counter<long> Failed = Meter.CreateCounter<long>("email.failed", description: "Email delivery attempts that failed and will be retried");
    private static readonly Counter<long> DeadLettered = Meter.CreateCounter<long>("email.deadlettered", description: "Emails that exhausted retries or failed permanently");
    private static readonly Histogram<double> SendDuration = Meter.CreateHistogram<double>("email.send.duration", unit: "ms", description: "Time spent handing an email to the transport");
    private static long _queueDepth;

    static EmailMetrics()
    {
        Meter.CreateObservableGauge("email.queue.depth", () => Interlocked.Read(ref _queueDepth), description: "Emails waiting to be delivered");
    }

    public static void RecordSent(string template, double elapsedMilliseconds)
    {
        var tag = new KeyValuePair<string, object?>("template", template);
        Sent.Add(1, tag);
        SendDuration.Record(elapsedMilliseconds, tag);
    }

    public static void RecordFailed(string template) => Failed.Add(1, new KeyValuePair<string, object?>("template", template));

    public static void RecordDeadLettered(string template) => DeadLettered.Add(1, new KeyValuePair<string, object?>("template", template));

    public static void SetQueueDepth(long depth) => Interlocked.Exchange(ref _queueDepth, depth);
}
