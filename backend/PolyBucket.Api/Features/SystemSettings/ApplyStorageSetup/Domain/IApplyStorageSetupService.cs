namespace PolyBucket.Api.Features.SystemSettings.ApplyStorageSetup.Domain;

public interface IApplyStorageSetupService
{
    Task<ApplyStorageSetupResponse> ApplyStorageSetupAsync(ApplyStorageSetupCommand request, CancellationToken cancellationToken = default);
}
