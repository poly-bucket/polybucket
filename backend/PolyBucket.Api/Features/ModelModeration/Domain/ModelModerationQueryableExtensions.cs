using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.ModelModeration.Domain;
using System.Linq;

namespace PolyBucket.Api.Features.ModelModeration.Domain;

public static class ModelModerationQueryableExtensions
{
    public static IQueryable<Model> WherePubliclyVisible(this IQueryable<Model> query, DbSet<ModelModerationRecord> moderationRecords)
    {
        return query.Where(m =>
            m.IsPublic
            && (!moderationRecords.Any(r => r.ModelId == m.Id)
                || moderationRecords.Any(r => r.ModelId == m.Id && r.Status == ModelModerationStatus.Approved)));
    }
}
