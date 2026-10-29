using System;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Tests.Testing;

namespace PolyBucket.Api.Tests.Features.Comments;

public static class CommentControllerTestContext
{
    public static T WithUser<T>(this T controller, Guid? userId, params string[] roles) where T : ControllerBase =>
        ControllerTestExtensions.WithUser(controller, userId, roles);
}
