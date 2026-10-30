using System;

namespace PolyBucket.Api.Features.Authentication.Register.Domain;

public sealed class UserRegistrationClosedException : Exception
{
    public const string DefaultMessage = "User registration is not enabled on this instance.";

    public UserRegistrationClosedException()
        : base(DefaultMessage)
    {
    }
}
