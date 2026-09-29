using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Repository;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using PolyBucket.Api.Features.Models.AddTagToModel.Domain;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Domain;

public class ModeratorEditModelService(
    IModeratorEditModelRepository repository,
    IModerationAuditLogWriter auditLogWriter) : IModeratorEditModelService
{
    private readonly IModeratorEditModelRepository _repository = repository;
    private readonly IModerationAuditLogWriter _auditLogWriter = auditLogWriter;

    public async Task<Model> EditModelAsync(
        Guid modelId,
        Guid moderatorId,
        ModeratorEditRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var model = await _repository.GetModelForEditAsync(modelId, cancellationToken);
        if (model == null)
        {
            throw new NotFoundException("Model not found");
        }

        var previousValues = new
        {
            model.Name,
            model.Description,
            model.License,
            model.Privacy,
            model.AIGenerated,
            model.WIP,
            model.NSFW,
            model.IsRemix,
            model.RemixUrl,
            model.IsPublic,
            model.IsFeatured,
            Tags = model.Tags.Select(t => t.Name).ToList(),
            Categories = model.Categories.Select(c => c.Name).ToList()
        };

        model.Name = request.Name;
        model.Description = request.Description;
        model.License = request.License;
        model.Privacy = request.Privacy;
        model.AIGenerated = request.AIGenerated;
        model.WIP = request.WIP;
        model.NSFW = request.NSFW;
        model.IsRemix = request.IsRemix;
        model.RemixUrl = request.RemixUrl;
        model.IsPublic = request.IsPublic;
        model.IsFeatured = request.IsFeatured;
        model.UpdatedAt = DateTime.UtcNow;

        if (request.Tags.Any())
        {
            model.Tags.Clear();
            foreach (var tagName in request.Tags.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                var existingTag = await _repository.FindTagByNameAsync(tagName.Trim(), cancellationToken);
                if (existingTag == null)
                {
                    existingTag = new Tag { Name = tagName.Trim() };
                    _repository.AddTag(existingTag);
                }

                model.Tags.Add(existingTag);
            }
        }

        if (request.Categories.Any())
        {
            model.Categories.Clear();
            foreach (var categoryName in request.Categories.Where(c => !string.IsNullOrWhiteSpace(c)))
            {
                var existingCategory = await _repository.FindCategoryByNameAsync(categoryName.Trim(), cancellationToken);
                if (existingCategory == null)
                {
                    existingCategory = new Category { Name = categoryName.Trim() };
                    _repository.AddCategory(existingCategory);
                }

                model.Categories.Add(existingCategory);
            }
        }

        var newValues = new
        {
            model.Name,
            model.Description,
            model.License,
            model.Privacy,
            model.AIGenerated,
            model.WIP,
            model.NSFW,
            model.IsRemix,
            model.RemixUrl,
            model.IsPublic,
            model.IsFeatured,
            Tags = request.Tags,
            Categories = request.Categories
        };

        await _repository.SaveChangesAsync(cancellationToken);

        await _auditLogWriter.WriteAsync(
            model.Id,
            moderatorId,
            request.Action,
            JsonSerializer.Serialize(previousValues),
            JsonSerializer.Serialize(newValues),
            request.ModerationNotes,
            ipAddress,
            userAgent,
            cancellationToken);

        return await _repository.GetModelForEditAsync(modelId, cancellationToken)
            ?? throw new NotFoundException("Model not found");
    }

    public Task<Model?> GetModelForModerationAsync(Guid modelId, CancellationToken cancellationToken = default)
    {
        return _repository.GetModelForEditAsync(modelId, cancellationToken);
    }
}
