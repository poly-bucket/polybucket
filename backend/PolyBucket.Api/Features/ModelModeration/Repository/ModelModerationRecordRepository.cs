using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ModelModeration.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.Repository;

public class ModelModerationRecordRepository(PolyBucketDbContext context) : IModelModerationRecordRepository
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<ModelModerationRecord?> GetByModelIdAsync(Guid modelId, CancellationToken cancellationToken = default)
    {
        return await _context.ModelModerationRecords
            .FirstOrDefaultAsync(r => r.ModelId == modelId, cancellationToken);
    }

    public async Task<bool> IsAuthorEmailVerifiedAsync(Guid authorId, CancellationToken cancellationToken = default)
    {
        var author = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == authorId, cancellationToken);
        if (author == null || string.IsNullOrEmpty(author.Email))
        {
            return false;
        }

        return await _context.EmailVerificationTokens
            .AsNoTracking()
            .AnyAsync(t => t.Email == author.Email && t.IsUsed, cancellationToken);
    }

    public async Task UpsertRecordAsync(ModelModerationRecord record, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ModelModerationRecords
            .FirstOrDefaultAsync(r => r.ModelId == record.ModelId, cancellationToken);

        if (existing == null)
        {
            _context.ModelModerationRecords.Add(record);
            return;
        }

        existing.Status = record.Status;
        existing.SubmittedAt = record.SubmittedAt;
        existing.ReviewedAt = record.ReviewedAt;
        existing.ReviewedByUserId = record.ReviewedByUserId;
        existing.RejectionReason = record.RejectionReason;
        existing.Notes = record.Notes;
    }

    public Task UpdateModelAsync(Model model, CancellationToken cancellationToken = default)
    {
        _context.Models.Update(model);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
