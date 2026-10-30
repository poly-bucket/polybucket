namespace PolyBucket.Api.Features.SystemSettings.SkipEmailSetup.Domain;

public interface ISkipEmailSetupService
{
    Task<SkipEmailSetupResponse> SkipEmailSetupAsync(SkipEmailSetupCommand request, CancellationToken cancellationToken = default);
}
