using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using PolyBucket.Api.Features.Comments.CreateComment.Http;
using PolyBucket.Api.Features.Comments.DeleteAllCommentsForTarget.Http;
using PolyBucket.Api.Features.Comments.DeleteComment.Http;
using PolyBucket.Api.Features.Comments.DislikeComment.Http;
using PolyBucket.Api.Features.Comments.GetComment.Http;
using PolyBucket.Api.Features.Comments.GetCommentsForTarget.Http;
using PolyBucket.Api.Features.Comments.GetCommentStatistics.Http;
using PolyBucket.Api.Features.Comments.GetModeratedComments.Http;
using PolyBucket.Api.Features.Comments.GetUserCommentStatistics.Http;
using PolyBucket.Api.Features.Comments.LikeComment.Http;
using PolyBucket.Api.Features.Comments.ModerateAllUserComments.Http;
using PolyBucket.Api.Features.Comments.ModerateComment.Http;
using PolyBucket.Api.Features.Comments.RemoveCommentDislike.Http;
using PolyBucket.Api.Features.Comments.RemoveCommentLike.Http;
using PolyBucket.Api.Features.Comments.ReportComment.Http;
using PolyBucket.Api.Features.Comments.UnmoderateComment.Http;
using PolyBucket.Api.Features.Comments.UpdateComment.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Comments;

public class CommentsControllerTests
{
    public static IEnumerable<object[]> ControllerRoutes() => new List<object[]>
    {
        new object[] { typeof(CreateCommentController), "POST", "" },
        new object[] { typeof(GetCommentsForTargetController), "GET", "target/{targetType}/{targetId:guid}" },
        new object[] { typeof(GetCommentController), "GET", "{commentId:guid}" },
        new object[] { typeof(UpdateCommentController), "PUT", "{commentId:guid}" },
        new object[] { typeof(DeleteCommentController), "DELETE", "{commentId:guid}" },
        new object[] { typeof(LikeCommentController), "POST", "{commentId:guid}/like" },
        new object[] { typeof(DislikeCommentController), "POST", "{commentId:guid}/dislike" },
        new object[] { typeof(RemoveCommentLikeController), "DELETE", "{commentId:guid}/like" },
        new object[] { typeof(RemoveCommentDislikeController), "DELETE", "{commentId:guid}/dislike" },
        new object[] { typeof(ReportCommentController), "POST", "{commentId:guid}/report" },
        new object[] { typeof(GetCommentStatisticsController), "GET", "statistics/{targetType}/{targetId:guid}" },
        new object[] { typeof(GetUserCommentStatisticsController), "GET", "user/{userId:guid}/statistics" },
        new object[] { typeof(ModerateCommentController), "POST", "{commentId:guid}/moderate" },
        new object[] { typeof(UnmoderateCommentController), "POST", "{commentId:guid}/unmoderate" },
        new object[] { typeof(GetModeratedCommentsController), "GET", "moderated" },
        new object[] { typeof(DeleteAllCommentsForTargetController), "DELETE", "target/{targetType}/{targetId:guid}" },
        new object[] { typeof(ModerateAllUserCommentsController), "POST", "user/{targetUserId:guid}/moderate-all" }
    };

    [Theory(DisplayName = "Each split comments controller keeps the original api/comments route and verb.")]
    [MemberData(nameof(ControllerRoutes))]
    public void Controller_KeepsOriginalRoute(Type controllerType, string verb, string template)
    {
        // Arrange
        var action = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Single();

        // Act
        var route = controllerType.GetCustomAttribute<RouteAttribute>();
        var http = action.GetCustomAttributes<HttpMethodAttribute>().Single();

        // Assert
        controllerType.GetCustomAttribute<ApiControllerAttribute>().ShouldNotBeNull();
        route.ShouldNotBeNull();
        route!.Template.ShouldBe("api/comments");
        http.HttpMethods.ShouldBe(new[] { verb });
        (http.Template ?? string.Empty).ShouldBe(template);
    }
}
