namespace PolyBucket.Api.Features.Authentication.DeleteAccount.Domain;

public class DeleteAccountResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static DeleteAccountResult Ok() =>
        new() { Success = true, Message = string.Empty };

    public static DeleteAccountResult Fail(string message) =>
        new() { Success = false, Message = message };
}
