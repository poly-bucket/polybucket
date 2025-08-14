using MediatR;
using PolyBucket.Api.Features.ThemeManagement.Repository;

namespace PolyBucket.Api.Features.ThemeManagement.SetActiveTheme;

public record SetActiveThemeCommand : IRequest<SetActiveThemeResponse>
{
    public int ThemeId { get; init; }
}

public record SetActiveThemeResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
}

public class SetActiveThemeCommandHandler : IRequestHandler<SetActiveThemeCommand, SetActiveThemeResponse>
{
    private readonly IThemeRepository _themeRepository;

    public SetActiveThemeCommandHandler(IThemeRepository themeRepository)
    {
        _themeRepository = themeRepository;
    }

    public async Task<SetActiveThemeResponse> Handle(SetActiveThemeCommand request, CancellationToken cancellationToken)
    {
        // Check if theme exists
        if (!await _themeRepository.ThemeExistsAsync(request.ThemeId))
        {
            return new SetActiveThemeResponse
            {
                Success = false,
                Message = "Theme not found."
            };
        }

        // Set theme as active
        var success = await _themeRepository.SetThemeAsActiveAsync(request.ThemeId);

        if (success)
        {
            return new SetActiveThemeResponse
            {
                Success = true,
                Message = "Theme activated successfully."
            };
        }

        return new SetActiveThemeResponse
        {
            Success = false,
            Message = "Failed to activate theme."
        };
    }
}
