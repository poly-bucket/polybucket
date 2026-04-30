using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Authentication.ForgotPassword.Http;
using PolyBucket.Api.Features.Authentication.Login.Http;
using PolyBucket.Api.Features.Authentication.OAuth.Http;
using PolyBucket.Api.Features.Authentication.RefreshToken.Http;
using PolyBucket.Api.Features.Authentication.Register.Http;
using PolyBucket.Api.Features.Authentication.ResetPassword.Http;
using PolyBucket.Api.Features.Authentication.VerifyEmail.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Authentication;

public class AuthenticationControllerTests
{
    public static IEnumerable<object[]> ControllerTypes()
    {
        return new List<object[]>
        {
            new object[] { typeof(LoginController) },
            new object[] { typeof(RegisterController) },
            new object[] { typeof(RefreshTokenController) },
            new object[] { typeof(ForgotPasswordController) },
            new object[] { typeof(ResetPasswordController) },
            new object[] { typeof(VerifyEmailController) },
            new object[] { typeof(OAuthController) }
        };
    }

    [Theory(DisplayName = "When inspecting authentication controllers, the controller has the ApiController and Route attributes applied")]
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