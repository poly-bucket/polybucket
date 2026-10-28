using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace PolyBucket.Api.Common.OpenApi;

/// <summary>
/// NSwag treats bare <see cref="IActionResult"/> actions without response metadata as binary downloads.
/// This processor restores JSON as the default produce type unless the action is explicitly documented as a file response.
/// </summary>
public sealed class OpenApiJsonResponseDefaultsProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var method = context.MethodInfo;
        if (method == null)
        {
            return true;
        }

        if (method.GetCustomAttributes<ProducesResponseTypeAttribute>(inherit: true).Any())
        {
            return true;
        }

        if (DeclaresBinaryResponse(method))
        {
            return true;
        }

        var operation = context.OperationDescription.Operation;
        operation.Produces.Clear();
        operation.Produces.Add("application/json");

        foreach (var response in operation.Responses.Values)
        {
            if (response.Content == null)
            {
                continue;
            }

            response.Content.Remove("application/octet-stream");

            if (response.Content.Count == 0)
            {
                response.Content["application/json"] = new OpenApiMediaType
                {
                    Schema = new JsonSchema { Type = JsonObjectType.Object }
                };
            }
        }

        return true;
    }

    private static bool DeclaresBinaryResponse(MethodInfo method)
    {
        foreach (var produces in method.GetCustomAttributes<ProducesAttribute>(inherit: true))
        {
            if (produces.ContentTypes.Any(IsBinaryContentType))
            {
                return true;
            }
        }

        foreach (var responseType in method.GetCustomAttributes<ProducesResponseTypeAttribute>(inherit: true))
        {
            if (responseType.Type == null)
            {
                continue;
            }

            if (typeof(FileResult).IsAssignableFrom(responseType.Type))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsBinaryContentType(string contentType)
    {
        return contentType.Contains("octet-stream", StringComparison.OrdinalIgnoreCase)
            || contentType.Equals("application/zip", StringComparison.OrdinalIgnoreCase);
    }
}
