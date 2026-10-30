using PolyBucket.Api.Features.Authentication.ExportAccount.Repository;

namespace PolyBucket.Api.Features.Authentication.ExportAccount.Domain;

public class ExportAccountService(IExportAccountRepository repository) : IExportAccountService
{
    private readonly IExportAccountRepository _repository = repository;

    public async Task<AccountExportResponse?> ExportAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _repository.GetUserForExportAsync(userId, cancellationToken);
        if (user == null)
        {
            return null;
        }

        return new AccountExportResponse
        {
            ExportedAtUtc = DateTime.UtcNow,
            UserId = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Bio = user.Bio,
            Country = user.Country,
            WebsiteUrl = user.WebsiteUrl,
            TwitterUrl = user.TwitterUrl,
            InstagramUrl = user.InstagramUrl,
            YouTubeUrl = user.YouTubeUrl,
            IsProfilePublic = user.IsProfilePublic,
            ShowEmail = user.ShowEmail,
            ShowLastLogin = user.ShowLastLogin,
            ShowStatistics = user.ShowStatistics,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            RoleName = user.Role?.Name,
            ModelCount = await _repository.GetModelCountAsync(userId, cancellationToken),
            CollectionCount = await _repository.GetCollectionCountAsync(userId, cancellationToken),
            FavoriteCollectionCount = await _repository.GetFavoriteCollectionCountAsync(userId, cancellationToken),
            RecentModels = (await _repository.GetRecentModelsAsync(userId, cancellationToken))
                .Select(m => new AccountExportModelSummaryDto
                {
                    ModelId = m.ModelId,
                    Name = m.Name,
                    CreatedAt = m.CreatedAt,
                    Privacy = m.Privacy,
                })
                .ToList(),
            RecentCollections = (await _repository.GetRecentCollectionsAsync(userId, cancellationToken))
                .Select(c => new AccountExportCollectionSummaryDto
                {
                    CollectionId = c.CollectionId,
                    Name = c.Name,
                    Visibility = c.Visibility,
                    CreatedAt = c.CreatedAt,
                })
                .ToList(),
            Settings = user.Settings == null
                ? null
                : new AccountExportSettingsDto
                {
                    Language = user.Settings.Language,
                    Theme = user.Settings.Theme,
                    EmailNotifications = user.Settings.EmailNotifications,
                    MeasurementSystem = user.Settings.MeasurementSystem,
                    TimeZone = user.Settings.TimeZone,
                },
        };
    }
}
