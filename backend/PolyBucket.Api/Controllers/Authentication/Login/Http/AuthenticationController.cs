using Api.Controllers.Authentication.Login.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Authentication.Login.Http
{
    public partial class AuthenticationController(
        CreateUserLoginService createUserLoginService,
        ILogger<AuthenticationController> logger) : ControllerBase
    {
        private readonly CreateUserLoginService _createUserLoginService = createUserLoginService;
        private readonly ILogger<AuthenticationController> _logger = logger;
    }
}