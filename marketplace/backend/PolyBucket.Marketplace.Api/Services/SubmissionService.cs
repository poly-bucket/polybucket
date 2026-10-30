using Microsoft.EntityFrameworkCore;
using PolyBucket.Marketplace.Api.Data;
using PolyBucket.Marketplace.Api.Models;

namespace PolyBucket.Marketplace.Api.Services;

public class SubmissionService(
    MarketplaceDbContext context,
    IPluginRepositoryManifestService manifestService,
    ILogger<SubmissionService> logger) : ISubmissionService
{
    public async Task<PluginSubmission> CreateSubmissionAsync(
        CreateSubmissionRequest request,
        string? submitterId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.RepositoryUrl) ||
            string.IsNullOrWhiteSpace(request.Category))
        {
            throw new InvalidOperationException("Name, category, and repository URL are required");
        }

        var submission = new PluginSubmission
        {
            Id = Guid.NewGuid().ToString(),
            SubmitterId = submitterId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Category = request.Category.Trim(),
            License = request.License?.Trim() ?? "MIT",
            RepositoryUrl = request.RepositoryUrl.Trim(),
            RepositoryBranch = string.IsNullOrWhiteSpace(request.RepositoryBranch) ? "main" : request.RepositoryBranch.Trim(),
            RepositorySubpath = request.RepositorySubpath?.Trim() ?? string.Empty,
            Status = "pending",
            SubmittedAt = DateTime.UtcNow
        };

        context.Submissions.Add(submission);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created plugin submission {SubmissionId}", submission.Id);
        return submission;
    }

    public async Task<List<SubmissionSummaryDto>> GetSubmissionsAsync(string? status, CancellationToken cancellationToken = default)
    {
        var query = context.Submissions.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(s => s.Status == status);
        }

        return await query
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => new SubmissionSummaryDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Category = s.Category,
                RepositoryUrl = s.RepositoryUrl,
                RepositoryBranch = s.RepositoryBranch,
                RepositorySubpath = s.RepositorySubpath,
                License = s.License,
                Status = s.Status,
                ReviewNotes = s.ReviewNotes,
                SubmittedAt = s.SubmittedAt,
                ReviewedAt = s.ReviewedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ApproveSubmissionResult> ApproveSubmissionAsync(string submissionId, CancellationToken cancellationToken = default)
    {
        var submission = await context.Submissions.FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);
        if (submission == null)
        {
            return new ApproveSubmissionResult { Success = false, Message = "Submission not found" };
        }

        if (submission.Status != "pending")
        {
            return new ApproveSubmissionResult { Success = false, Message = "Only pending submissions can be approved" };
        }

        var manifestResult = await manifestService.ReadManifestFromRepositoryAsync(
            submission.RepositoryUrl,
            submission.RepositoryBranch,
            submission.RepositorySubpath,
            cancellationToken);

        if (!manifestResult.Success || manifestResult.Manifest == null)
        {
            return new ApproveSubmissionResult
            {
                Success = false,
                Message = manifestResult.Message
            };
        }

        var manifest = manifestResult.Manifest;
        var targets = string.Join(',', manifest.Targets.Select(t => t.Trim().ToLowerInvariant()).Distinct());

        var plugin = await context.Plugins.FirstOrDefaultAsync(p => p.Id == manifest.Id, cancellationToken);
        if (plugin == null)
        {
            plugin = new Plugin
            {
                Id = manifest.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Plugins.Add(plugin);
        }

        plugin.Name = manifest.Name;
        plugin.Description = manifest.Description;
        plugin.LongDescription = submission.Description;
        plugin.Category = submission.Category;
        plugin.Version = manifest.Version;
        plugin.RepositoryUrl = submission.RepositoryUrl;
        plugin.RepositoryBranch = submission.RepositoryBranch;
        plugin.RepositorySubpath = submission.RepositorySubpath;
        plugin.Targets = targets;
        plugin.License = string.IsNullOrWhiteSpace(manifest.License) ? submission.License : manifest.License;
        plugin.Status = "published";
        plugin.IsActive = true;
        plugin.IsVerified = true;
        plugin.UpdatedAt = DateTime.UtcNow;

        submission.Status = "published";
        submission.ReviewedAt = DateTime.UtcNow;
        submission.PublishedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new ApproveSubmissionResult
        {
            Success = true,
            Message = "Submission approved and plugin published",
            PluginId = plugin.Id
        };
    }

    public async Task<SubmissionSummaryDto?> RejectSubmissionAsync(
        string submissionId,
        RejectSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var submission = await context.Submissions.FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);
        if (submission == null)
        {
            return null;
        }

        if (submission.Status != "pending")
        {
            throw new InvalidOperationException("Only pending submissions can be rejected");
        }

        submission.Status = "rejected";
        submission.ReviewNotes = request.ReviewNotes?.Trim();
        submission.ReviewedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new SubmissionSummaryDto
        {
            Id = submission.Id,
            Name = submission.Name,
            Description = submission.Description,
            Category = submission.Category,
            RepositoryUrl = submission.RepositoryUrl,
            RepositoryBranch = submission.RepositoryBranch,
            RepositorySubpath = submission.RepositorySubpath,
            License = submission.License,
            Status = submission.Status,
            ReviewNotes = submission.ReviewNotes,
            SubmittedAt = submission.SubmittedAt,
            ReviewedAt = submission.ReviewedAt
        };
    }
}
