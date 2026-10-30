using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.ApplyTheme.Domain;

public enum ThemeRequestStatus { Ok, BadRequest }
public sealed class ThemeSimpleOutcome<T> { public ThemeRequestStatus Status { get; init; } public T? Value { get; init; } public object? ErrorBody { get; init; } }

public interface IApplyThemeService
{
    ThemeSimpleOutcome<ThemeApplicationResult> Apply(string pluginId);
}
