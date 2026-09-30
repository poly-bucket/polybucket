using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PolyBucket.Api.Features.Models.Http;

public static class ModelDownloadCountHeaderExtensions
{
    public static void ApplyModelDownloadCountHeaders(this HttpResponse response, int downloads, bool counted)
    {
        response.Headers[ModelDownloadCountHeaders.Downloads] = downloads.ToString();
        response.Headers[ModelDownloadCountHeaders.Counted] = counted ? "true" : "false";
    }

    public static FileResult WithModelDownloadCountHeaders(this ControllerBase controller, FileResult file, int downloads, bool counted)
    {
        controller.Response.ApplyModelDownloadCountHeaders(downloads, counted);
        return file;
    }
}
