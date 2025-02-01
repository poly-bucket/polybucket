using Database;
using Microsoft.EntityFrameworkCore;
using Core.Models.Users.Settings;

namespace Api.Controllers.Users.UserSettings.Domain;

public interface IGetUserSettingsService
{
    Task<GetUserSettingsResponse> ExecuteAsync(GetUserSettingsRequest request);
}

public class GetUserSettingsService : IGetUserSettingsService
{
    private readonly Context _context;
    private readonly ILogger<GetUserSettingsService> _logger;

    public GetUserSettingsService(Context context, ILogger<GetUserSettingsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetUserSettingsResponse> ExecuteAsync(GetUserSettingsRequest request)
    {
        var settings = await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == request.UserId);

        if (settings == null)
        {
            throw new KeyNotFoundException($"Settings not found for user {request.UserId}");
        }

        return new GetUserSettingsResponse
        {
            Settings = settings
        };
    }
} 