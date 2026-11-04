using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.Authentication.RecoveryCodeResetPassword.Domain;
using PolyBucket.Api.Features.Authentication.RecoveryCodeResetPassword.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Database.Entities.Authentication;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Email;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication.RecoveryCodeResetPassword;

public class RecoveryCodeResetPasswordCommandHandlerTests
{
    [Fact(DisplayName = "When the recovery code is invalid, the handler throws with a generic message.")]
    public async Task Handle_InvalidCode_Throws()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "user@test.com", Username = "user" };
        var repository = new Mock<IRecoveryCodeResetPasswordRepository>();
        repository.Setup(r => r.FindUserByEmailOrUsernameAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var recoveryRepository = new Mock<IRecoveryCodeRepository>();
        recoveryRepository.Setup(r => r.GetUnusedCodesForUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AccountRecoveryCode>());

        var recoveryService = new RecoveryCodeService(
            Microsoft.Extensions.Options.Options.Create(new PolyBucket.Api.Features.Authentication.Domain.RecoveryCodeSettings()),
            new RecoveryCodeProtector(new EphemeralDataProtectionProvider()));

        var handler = new RecoveryCodeResetPasswordCommandHandler(
            repository.Object,
            recoveryRepository.Object,
            recoveryService,
            Mock.Of<IPasswordHasher>(),
            Mock.Of<IAccountEmailService>(),
            TimeProvider.System,
            Mock.Of<ILogger<RecoveryCodeResetPasswordCommandHandler>>());

        // Act
        var act = () => handler.Handle(new RecoveryCodeResetPasswordCommand
        {
            EmailOrUsername = "user@test.com",
            RecoveryCode = "NOT-A-VALID-CODE",
            NewPassword = "NewPassword123!"
        }, CancellationToken.None);

        // Assert
        var ex = await Should.ThrowAsync<InvalidOperationException>(act);
        ex.Message.ShouldBe(RecoveryCodeResetPasswordCommandHandler.InvalidRequestMessage);
    }
}
