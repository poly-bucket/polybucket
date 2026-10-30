using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Federation.ImportFederatedModel.Http;
using PolyBucket.Api.Features.Federation.ImportFederatedModel.Repository;

namespace PolyBucket.Api.Features.Federation.ImportFederatedModel.Domain;

public interface IImportFederatedModelService
{
    Task<ActionResult<ImportFederatedModelResponse>> ImportFederatedModelAsync(
        ImportFederatedModelRequest request,
        CancellationToken cancellationToken = default);
}

public class ImportFederatedModelService(IImportFederatedModelRepository repository) : IImportFederatedModelService
{
    public async Task<ActionResult<ImportFederatedModelResponse>> ImportFederatedModelAsync(
        ImportFederatedModelRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.InstanceId))
        {
            return new BadRequestObjectResult("Instance ID is required");
        }

        if (string.IsNullOrWhiteSpace(request.RemoteModelId))
        {
            return new BadRequestObjectResult("Remote model ID is required");
        }

        try
        {
            var model = await repository.ImportModelAsync(request.InstanceId, request.RemoteModelId, cancellationToken);
            return new CreatedAtActionResult(
                nameof(GetFederatedModels.Http.GetFederatedModelsController.GetFederatedModels),
                "GetFederatedModels",
                null,
                new ImportFederatedModelResponse
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    RemoteInstanceId = model.RemoteInstanceId,
                    RemoteModelId = model.RemoteModelId,
                    IsFederated = model.IsFederated,
                    LastFederationSync = model.LastFederationSync,
                    CreatedAt = model.CreatedAt
                });
        }
        catch (InvalidOperationException ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            return new ObjectResult($"Failed to import model: {ex.Message}") { StatusCode = 500 };
        }
    }
}
