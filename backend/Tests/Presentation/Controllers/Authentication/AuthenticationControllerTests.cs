using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Presentation.Controllers.Authentication
{
    public class AuthenticationControllerTests
    {
        #region Login

        [Fact]
        [Description("Given valid credentials, When Login is called, Then it should return OKObjectResult")]
        public async Task Login_ValidCredentials_ReturnsOkResult()
        {
            // Arrange
            //var authenticationServiceMock = new Mock<IAuthenticationService>();
            var controller = new Api.Controllers.Authentication();
            var username = "admin";
            var password = "admin";

            // Act
            var result = controller.Login(username, password);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion Login
    }
}