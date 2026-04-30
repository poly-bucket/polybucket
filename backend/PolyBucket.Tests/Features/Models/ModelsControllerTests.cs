using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Http;
using PolyBucket.Api.Features.Models.AddModelToCollection.Http;
using PolyBucket.Api.Features.Models.AddTagToModel.Http;
using PolyBucket.Api.Features.Models.CreateModel.Http;
using PolyBucket.Api.Features.Models.CreateModelVersion.Http;
using PolyBucket.Api.Features.Models.DeleteAllModels.Http;
using PolyBucket.Api.Features.Models.DeleteModel.Http;
using PolyBucket.Api.Features.Models.DeleteModelVersion.Http;
using PolyBucket.Api.Features.Models.DownloadModel.Http;
using PolyBucket.Api.Features.Models.GenerateCustomThumbnail.Http;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Http;
using PolyBucket.Api.Features.Models.GetModelById.Http;
using PolyBucket.Api.Features.Models.GetModelPreview.Http;
using PolyBucket.Api.Features.Models.GetModels.Http;
using PolyBucket.Api.Features.Models.GetModelByUserId.Http;
using PolyBucket.Api.Features.Models.GetModelVersions.Http;
using PolyBucket.Api.Features.Models.LikeModel.Http;
using PolyBucket.Api.Features.Models.RemoveCategoryFromModel.Http;
using PolyBucket.Api.Features.Models.RemoveModelFromCollection.Http;
using PolyBucket.Api.Features.Models.RemoveTagFromModel.Http;
using PolyBucket.Api.Features.Models.UpdateModel.Http;
using PolyBucket.Api.Features.Models.UpdateModelVersion.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models;

public class ModelsControllerTests : IDisposable
{
    public static IEnumerable<object[]> ControllerTypes()
    {
        return new List<object[]>
        {
            new object[] { typeof(AddCategoryToModelController) },
            new object[] { typeof(AddModelToCollectionController) },
            new object[] { typeof(AddTagToModelController) },
            new object[] { typeof(CreateModelController) },
            new object[] { typeof(CreateModelVersionController) },
            new object[] { typeof(DeleteAllModelsController) },
            new object[] { typeof(DeleteModelController) },
            new object[] { typeof(DeleteModelVersionController) },
            new object[] { typeof(DownloadModelController) },
            new object[] { typeof(GenerateCustomThumbnailController) },
            new object[] { typeof(GenerateModelPreviewController) },
            new object[] { typeof(GetModelByIdController) },
            new object[] { typeof(GetModelPreviewController) },
            new object[] { typeof(GetModelsController) },
            new object[] { typeof(GetModelByUserIdController) },
            new object[] { typeof(GetModelVersionsController) },
            new object[] { typeof(LikeModelController) },
            new object[] { typeof(RemoveCategoryFromModelController) },
            new object[] { typeof(RemoveModelFromCollectionController) },
            new object[] { typeof(RemoveTagFromModelController) },
            new object[] { typeof(UpdateModelController) },
            new object[] { typeof(UpdateModelVersionController) }
        };
    }

    [Theory(DisplayName = "When inspecting model controllers, the controller has the ApiController and Route attributes applied")]
    [MemberData(nameof(ControllerTypes))]
    public void Controller_ShouldHaveApiControllerAndRoute(Type controllerType)
    {
        // Ensure ApiController attribute exists
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        apiAttr.ShouldNotBeNull();

        // Ensure RouteAttribute exists
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        routeAttr.ShouldNotBeNull();
    }

    public void Dispose()
    {
        // No resources to dispose in this test class
    }
} 