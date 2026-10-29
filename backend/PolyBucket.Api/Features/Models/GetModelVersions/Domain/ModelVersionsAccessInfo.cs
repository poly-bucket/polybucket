using System;
using PolyBucket.Database.Entities.Models.Enums;

namespace PolyBucket.Api.Features.Models.GetModelVersions.Domain;

public record ModelVersionsAccessInfo(Guid AuthorId, PrivacySettings Privacy, bool IsPublic, bool PassedModeration);
