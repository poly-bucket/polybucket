using Microsoft.AspNetCore.Http;

namespace PolyBucket.Api.Features.Files.StreamModelFile.Http;

public static class StreamModelFileDownloadCountHeaders
{
    public const string Downloads = "X-Model-Downloads";
    public const string Counted = "X-Model-Download-Counted";
}

public static class StreamModelFileDownloadCountHeaderExtensions
{
    public static void ApplyModelDownloadCountHeaders(this HttpResponse response, int downloads, bool counted)
    {
        response.Headers[StreamModelFileDownloadCountHeaders.Downloads] = downloads.ToString();
        response.Headers[StreamModelFileDownloadCountHeaders.Counted] = counted ? "true" : "false";
    }
}
