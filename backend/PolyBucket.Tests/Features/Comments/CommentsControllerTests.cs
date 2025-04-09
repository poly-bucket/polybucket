using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Comments.Commands;
using PolyBucket.Api.Features.Comments.Queries;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments;

public class CommentsControllerTests
{
    public static IEnumerable<object[]> ControllerTypes()
    {
        return new List<object[]>
        {
            new object[] { typeof(AddCommentController) },
            new object[] { typeof(DeleteCommentController) },
            new object[] { typeof(DislikeCommentController) },
            new object[] { typeof(LikeCommentController) },
            new object[] { typeof(GetCommentsForModelController) }
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
} 