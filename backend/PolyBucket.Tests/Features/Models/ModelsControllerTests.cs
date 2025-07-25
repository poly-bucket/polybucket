using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Models.GetModelById.Http;
using PolyBucket.Api.Features.Models.GetModels.Http;
using PolyBucket.Api.Features.Models.Http;
using PolyBucket.Api.Features.Models.CreateModel.Http;
using Shouldly;
using Xunit;
using PolyBucket.Api.Features.Models.CreateModelVersion.Http;
using PolyBucket.Api.Features.Models.UpdateModel.Http;
using PolyBucket.Api.Features.Models.DeleteModel.Http;
using PolyBucket.Api.Features.Models.GetModelByUserId.Http;

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
            new object[] { typeof(CreateModelVersionController) },
            new object[] { typeof(DeleteModelController) },
            new object[] { typeof(DownloadModelController) },
            new object[] { typeof(GetModelByIdController) },
            new object[] { typeof(GetModelsController) },
            new object[] { typeof(GetModelByUserIdController) },
            new object[] { typeof(GetModelVersionsController) },
            new object[] { typeof(LikeModelController) },
            new object[] { typeof(RemoveCategoryFromModelController) },
            new object[] { typeof(RemoveModelFromCollectionController) },
            new object[] { typeof(RemoveTagFromModelController) },
            new object[] { typeof(SearchModelsController) },
            new object[] { typeof(UpdateModelController) },
            new object[] { typeof(CreateModelController) }
        };
    }

    [Theory]
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