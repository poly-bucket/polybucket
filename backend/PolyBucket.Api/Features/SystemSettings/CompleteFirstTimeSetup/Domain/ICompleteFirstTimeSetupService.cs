namespace PolyBucket.Api.Features.SystemSettings.CompleteFirstTimeSetup.Domain;

public interface ICompleteFirstTimeSetupService
{
    Task<CompleteFirstTimeSetupResponse> CompleteFirstTimeSetupAsync(CompleteFirstTimeSetupCommand request, CancellationToken cancellationToken = default);
}
