namespace PolyBucket.Api.Features.Authentication.Account.Domain;

public class DeleteOwnAccountResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }

    public static DeleteOwnAccountResult Ok() =>
        new() { Success = true };

    public static DeleteOwnAccountResult Fail(string message) =>
        new() { Success = false, Message = message };
}
