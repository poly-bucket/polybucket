namespace PolyBucket.Api.Features.SystemSettings.GetFontAwesomeProEnabled.Domain;

public interface IGetFontAwesomeProEnabledService
{
    Task<bool> IsFontAwesomeProEnabledAsync(CancellationToken cancellationToken = default);
}
