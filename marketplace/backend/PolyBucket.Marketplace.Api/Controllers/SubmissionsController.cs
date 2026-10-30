using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Marketplace.Api.Models;
using PolyBucket.Marketplace.Api.Services;
using System.Security.Claims;

namespace PolyBucket.Marketplace.Api.Controllers;

[ApiController]
[Route("api/submissions")]
[Produces("application/json")]
public class SubmissionsController(
    ISubmissionService submissionService,
    ILogger<SubmissionsController> logger) : ControllerBase
{
    /// <summary>
    /// Submit a plugin repository for marketplace review.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SubmissionSummaryDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<SubmissionSummaryDto>> CreateSubmission(
        [FromBody] CreateSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var submitterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var submission = await submissionService.CreateSubmissionAsync(request, submitterId, cancellationToken);
            var dto = new SubmissionSummaryDto
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
                SubmittedAt = submission.SubmittedAt
            };
            return CreatedAtAction(nameof(GetSubmissions), new { status = "pending" }, dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating submission");
            return StatusCode(500, new { message = "Error creating submission" });
        }
    }

    /// <summary>
    /// List plugin submissions, optionally filtered by status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<SubmissionSummaryDto>), 200)]
    public async Task<ActionResult<List<SubmissionSummaryDto>>> GetSubmissions(
        [FromQuery] string? status = "pending",
        CancellationToken cancellationToken = default)
    {
        var items = await submissionService.GetSubmissionsAsync(status, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Approve a pending submission and publish the plugin listing.
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(ApproveSubmissionResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ApproveSubmissionResult>> ApproveSubmission(string id, CancellationToken cancellationToken)
    {
        var result = await submissionService.ApproveSubmissionAsync(id, cancellationToken);
        if (!result.Success && result.Message == "Submission not found")
        {
            return NotFound(result);
        }

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Reject a pending submission.
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(SubmissionSummaryDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<SubmissionSummaryDto>> RejectSubmission(
        string id,
        [FromBody] RejectSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await submissionService.RejectSubmissionAsync(id, request, cancellationToken);
            if (result == null)
            {
                return NotFound(new { message = "Submission not found" });
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
