using PolyBucket.Marketplace.Api.Models;

namespace PolyBucket.Marketplace.Api.Services;

public interface ISubmissionService
{
    Task<PluginSubmission> CreateSubmissionAsync(CreateSubmissionRequest request, string? submitterId, CancellationToken cancellationToken = default);
    Task<List<SubmissionSummaryDto>> GetSubmissionsAsync(string? status, CancellationToken cancellationToken = default);
    Task<ApproveSubmissionResult> ApproveSubmissionAsync(string submissionId, CancellationToken cancellationToken = default);
    Task<SubmissionSummaryDto?> RejectSubmissionAsync(string submissionId, RejectSubmissionRequest request, CancellationToken cancellationToken = default);
}
