using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.GetFileSettings.Repository;

public class GetFileSettingsRepository(PolyBucketDbContext context) : IGetFileSettingsRepository
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<GetFileSettingsResponse> GetFileSettingsAsync(
        GetFileSettingsQueryParams query,
        CancellationToken cancellationToken)
    {
        var allCategories = await _context.FileTypeSettings
            .AsNoTracking()
            .Where(ft => ft.DeletedAt == null && ft.Category != null && ft.Category != string.Empty)
            .Select(ft => ft.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);

        var systemSetup = await _context.SystemSetups.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        var siteAllowed = systemSetup?.AllowedFileTypes ?? string.Empty;

        var enabledForDerived = await _context.FileTypeSettings
            .AsNoTracking()
            .Where(ft => ft.DeletedAt == null && ft.Enabled)
            .OrderBy(ft => ft.Category)
            .ThenBy(ft => ft.Priority)
            .ThenBy(ft => ft.FileExtension)
            .ToListAsync(cancellationToken);

        var derivedAllowed = AllowedFileTypesAlignment.BuildDerivedAllowedFileTypes(enabledForDerived);
        var inSync = AllowedFileTypesAlignment.AreExtensionSetsInSync(siteAllowed, derivedAllowed);

        var queryable = _context.FileTypeSettings
            .AsNoTracking()
            .Where(ft => ft.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            queryable = queryable.Where(ft =>
                ft.FileExtension.ToLower().Contains(term) ||
                ft.DisplayName.ToLower().Contains(term) ||
                ft.MimeType.ToLower().Contains(term) ||
                ft.Description.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(query.Category) &&
            !string.Equals(query.Category, "All", StringComparison.OrdinalIgnoreCase))
        {
            var category = query.Category.Trim();
            queryable = queryable.Where(ft => ft.Category == category);
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / query.PageSize);

        var rows = await (
                from ft in queryable
                join creator in _context.Users on ft.CreatedById equals creator.Id into creatorJoin
                from creator in creatorJoin.DefaultIfEmpty()
                join updater in _context.Users on ft.UpdatedById equals updater.Id into updaterJoin
                from updater in updaterJoin.DefaultIfEmpty()
                orderby ft.Category, ft.Priority, ft.FileExtension
                select new
                {
                    FileType = ft,
                    CreatorUsername = creator != null ? creator.Username : null,
                    CreatorEmail = creator != null ? creator.Email : null,
                    UpdaterUsername = updater != null ? updater.Username : null
                })
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var fileTypes = rows.Select(row =>
        {
            var ft = row.FileType;
            return new FileTypeSettingsData
            {
                Id = ft.Id,
                FileExtension = ft.FileExtension,
                Enabled = ft.Enabled,
                MaxFileSizeBytes = ft.MaxFileSizeBytes,
                MaxPerUpload = ft.MaxPerUpload,
                DisplayName = ft.DisplayName,
                Description = ft.Description,
                MimeType = ft.MimeType,
                RequiresPreview = ft.RequiresPreview,
                IsCompressible = ft.IsCompressible,
                Category = ft.Category,
                Priority = ft.Priority,
                IsDefault = ft.IsDefault,
                CreatedAt = ft.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = ft.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty,
                CreatedByUsername = row.CreatorUsername ?? "Unknown",
                CreatedByEmail = row.CreatorEmail ?? string.Empty,
                UpdatedByUsername = row.UpdaterUsername ?? string.Empty
            };
        }).ToList();

        return new GetFileSettingsResponse
        {
            Success = true,
            Message = "File type settings retrieved successfully",
            FileTypes = fileTypes,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = totalPages,
            HasNextPage = query.Page < totalPages,
            HasPreviousPage = query.Page > 1,
            Categories = allCategories,
            SiteAllowedFileTypes = siteAllowed,
            DerivedAllowedFileTypes = derivedAllowed,
            AllowedFileTypesInSync = inSync
        };
    }
}
