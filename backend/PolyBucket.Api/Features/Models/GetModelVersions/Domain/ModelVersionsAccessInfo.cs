using System;
using PolyBucket.Api.Common.Models.Enums;

namespace PolyBucket.Api.Features.Models.GetModelVersions.Domain;

public record ModelVersionsAccessInfo(Guid AuthorId, PrivacySettings Privacy, bool IsPublic, bool PassedModeration);
