using PolyBucket.Api.Features.Plugins.Domain;

namespace PolyBucket.Api.Features.Plugins.RemoveTheme.Repository;

public interface IRemoveThemeRepository { Task<ThemeApplicationResult> RemoveThemeAsync(string pluginId); }
