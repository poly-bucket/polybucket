using PolyBucket.Api.Features.Plugins.GetPlugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetPlugins.Http;

public static class GetPluginsMapper
{
    public static IEnumerable<GetPluginsResponseDto> ToDto(IEnumerable<GetPluginsItem> items) =>
        items.Select(i => new GetPluginsResponseDto
        {
            Id = i.Id,
            Name = i.Name,
            Version = i.Version,
            Author = i.Author,
            Description = i.Description
        });
}
