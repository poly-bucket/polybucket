namespace PolyBucket.Api.Features.SystemSettings.CheckFirstTimeSetup.Domain;

public interface ICheckFirstTimeSetupService
{
    Task<CheckFirstTimeSetupResponse> CheckFirstTimeSetupAsync(CheckFirstTimeSetupQuery request, CancellationToken cancellationToken = default);
}
